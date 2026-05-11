#!/usr/bin/env bash
# Helper for local K3s dev: pod status, describe, logs, port-forwards.
# Run from Ubuntu/WSL with kubectl configured for your cluster.

set -euo pipefail

NS_APP="jk-three-tier"
NS_OBS="observability"

# Deployments we usually care about in the app namespace (label app= matches).
BACKEND_DEPLOYS=(configuration messaging offer order frontend postgres)

usage() {
  cat <<EOF
Usage: $(basename "$0") <command> [args]

App namespace: ${NS_APP}
Observability namespace: ${NS_OBS}

Commands:
  -pods, --pods           List pods in ${NS_APP} and ${NS_OBS} (-o wide), plus Deployments/StatefulSets
                          with replicas=0 (they do not create Pods, so they never appear in pod lists)
  -pods-all, --pods-all   List pods in all namespaces (-o wide)

  -deploy, --deploy       List Deployments (-o wide): ${NS_APP}, then ${NS_OBS}
                          (what you asked Kubernetes to run; desired vs ready replicas)

  -rollout, --rollout [app]
                          Without app: rollout status for known backend Deployments (90s timeout each).
                          With app: rollout status for deployment/<app> only.
                          Use after image changes / restarts to confirm new Pods became Ready.

  -top, --top             CPU/memory use for Pods (${NS_APP}, then ${NS_OBS} if present).
                          Needs Metrics Server (see -metrics-check). Fails with hints if missing.

  -metrics-check, --metrics-check
                          Read-only checks: is metrics.k8s.io registered? metrics pods in kube-system?
                          Use when kubectl top fails or you are unsure Metrics Server exists.

  -d, --describe <app>    kubectl describe pods -l app=<app> -n ${NS_APP}
  -l, --logs <app> [-f]   Last 200 log lines (-n ${NS_APP}); add -f to follow

  -pf, --port-forward <target>   kubectl port-forward (blocks until Ctrl+C)
      grafana | logs    Grafana UI → http://localhost:3000  (svc/grafana:80)
      loki              Loki HTTP → localhost:3100 (svc/loki, matches your cluster)
      alloy             Grafana Alloy UI → http://localhost:12345  (svc/alloy:12345)
      configuration     HTTP/Swagger → http://localhost:9084/swagger
      messaging         HTTP/Swagger → http://localhost:9083/swagger
      offer             HTTP/Swagger → http://localhost:9082/swagger
      order             HTTP/Swagger → http://localhost:9081/swagger
      frontend          Web UI → http://localhost:9080/
      postgres          Postgres → localhost:15432 (psql -h 127.0.0.1 -p 15432 ...)

  -svc, --services       Services in ${NS_APP} and ${NS_OBS}
  -events, --events      Recent events in ${NS_APP} (sorted by time)

  -s, --scale <name> [n]
                          Scale workload in ${NS_APP} to n replicas (default n=1 if omitted).
                          <name> can be a Deployment, StatefulSet, or Pod name (Pods are resolved
                          to their owning Deployment via ReplicaSet, or to their StatefulSet).

Apps for -d / -l (label app=): configuration, messaging, offer, order, frontend, postgres

  -h, --help             Show this help

Examples:
  $(basename "$0") -pods
  $(basename "$0") -deploy
  $(basename "$0") -rollout
  $(basename "$0") -rollout order
  $(basename "$0") -metrics-check
  $(basename "$0") -top
  $(basename "$0") -d order
  $(basename "$0") -l offer
  $(basename "$0") -l messaging -f
  $(basename "$0") -pf grafana
  $(basename "$0") -pf alloy
  $(basename "$0") -pf order
  $(basename "$0") -s order 0
  $(basename "$0") -s messaging
  $(basename "$0") -s order-774d8f9b6d-xk2zp 2
EOF
}

have_ns() {
  kubectl get ns "$1" &>/dev/null
}

# Workloads with replicas=0 own no Pods — kubectl get pods cannot list them.
show_scaled_to_zero() {
  local ns="$1"
  local deploys sts
  deploys=$(kubectl get deploy -n "${ns}" -o jsonpath='{range .items[?(@.spec.replicas==0)]}{.metadata.name}{"\n"}{end}' 2>/dev/null || true)
  sts=$(kubectl get sts -n "${ns}" -o jsonpath='{range .items[?(@.spec.replicas==0)]}{.metadata.name}{"\n"}{end}' 2>/dev/null || true)

  echo "=== Scaled to 0 replicas (no Pods): ${ns} ==="
  if [[ -z "${deploys}" && -z "${sts}" ]]; then
    echo "(none)"
    echo
    return
  fi
  if [[ -n "${deploys}" ]]; then
    echo "Deployments:"
    echo "${deploys}" | sed 's/^/  /'
  fi
  if [[ -n "${sts}" ]]; then
    echo "StatefulSets:"
    echo "${sts}" | sed 's/^/  /'
  fi
  echo
}

cmd_pods() {
  echo "=== Pods: ${NS_APP} ==="
  kubectl get pods -n "${NS_APP}" -o wide
  echo
  show_scaled_to_zero "${NS_APP}"

  echo "=== Pods: ${NS_OBS} ==="
  if have_ns "${NS_OBS}"; then
    kubectl get pods -n "${NS_OBS}" -o wide
    echo
    show_scaled_to_zero "${NS_OBS}"
  else
    echo "(namespace ${NS_OBS} not found — deploy observability stack or ignore)"
    echo
  fi
}

cmd_pods_all() {
  kubectl get pods -A -o wide
}

cmd_deploy() {
  echo "=== Deployments: ${NS_APP} ==="
  kubectl get deploy -n "${NS_APP}" -o wide
  echo
  echo "=== Deployments: ${NS_OBS} ==="
  if have_ns "${NS_OBS}"; then
    kubectl get deploy -n "${NS_OBS}" -o wide
  else
    echo "(namespace ${NS_OBS} not found)"
  fi
}

cmd_rollout_one() {
  local app="$1"
  echo "=== rollout status deployment/${app} (${NS_APP}) ==="
  kubectl rollout status "deployment/${app}" -n "${NS_APP}" --timeout=90s
}

cmd_rollout_all() {
  local d
  for d in "${BACKEND_DEPLOYS[@]}"; do
    if kubectl get deployment "${d}" -n "${NS_APP}" &>/dev/null; then
      cmd_rollout_one "${d}"
      echo
    else
      echo "=== skip deployment/${d} (not found) ==="
      echo
    fi
  done
}

cmd_top_fail_hints() {
  cat >&2 <<'EOF'

kubectl top failed. Common causes:
  • Metrics Server is not installed, or
  • It was installed recently and has not scraped nodes yet (wait ~1 minute).

What Metrics Server is for:
  • Supplies live CPU/memory metrics so kubectl top works.
  • Same metrics feed the Horizontal Pod Autoscaler (not used everywhere on dev clusters).

How to verify (also available as: jk-k3s-helper.sh -metrics-check):
  kubectl get apiservice v1beta1.metrics.k8s.io -o wide
    # AVAILABLE=True means the API is wired up.

  kubectl api-resources | grep -i metrics

  kubectl get pods -n kube-system | grep -i metrics
    # k3s may ship without it; install via Helm/k3s addon if you want kubectl top.

EOF
}

cmd_top() {
  if kubectl top pods -n "${NS_APP}"; then
    :
  else
    cmd_top_fail_hints
    exit 1
  fi
  echo
  if have_ns "${NS_OBS}"; then
    echo "=== Top pods: ${NS_OBS} ==="
    kubectl top pods -n "${NS_OBS}" || echo "(top failed for ${NS_OBS} — same metrics API as above)"
  fi
}

cmd_metrics_check() {
  cat <<EOF
=== Metrics API registration ===
EOF
  kubectl get apiservice 2>/dev/null | grep -E 'NAME|metrics' || echo "(no metrics rows in apiservice list)"

  echo
  echo "=== v1beta1.metrics.k8s.io (detail) ==="
  if kubectl get apiservice v1beta1.metrics.k8s.io -o wide 2>/dev/null; then
    :
  else
    echo "Not found — Metrics Server is likely not installed or not registered."
  fi

  echo
  echo "=== kube-system pods with 'metrics' in name ==="
  kubectl get pods -n kube-system 2>/dev/null | grep -i metrics || echo "(none — expected if Metrics Server not installed)"

  echo
  echo "=== Optional: any metrics-related deployments in kube-system ==="
  kubectl get deploy -n kube-system -o name 2>/dev/null | grep -i metrics || echo "(none)"

  echo
  cat <<EOF
If AVAILABLE is False on the apiservice: wait, or check Metrics Server pod logs.
k3s docs often describe enabling metrics-server if you want kubectl top on single-node dev clusters.
EOF
}

cmd_describe() {
  local app="$1"
  kubectl describe pods -n "${NS_APP}" -l "app=${app}"
}

cmd_logs() {
  local app="$1"
  local follow="${2:-false}"
  local -a args=(kubectl logs -n "${NS_APP}" -l "app=${app}" --tail=200)
  [[ "${follow}" == true ]] && args+=(-f)
  "${args[@]}"
}

cmd_pf() {
  local raw="$1"
  local t
  t=$(echo "${raw}" | tr '[:upper:]' '[:lower:]')
  case "${t}" in
    grafana | logs)
      echo "Forwarding Grafana → http://localhost:3000  (namespace ${NS_OBS})"
      kubectl port-forward -n "${NS_OBS}" svc/grafana 3000:80
      ;;
    loki)
      echo "Forwarding Loki (HTTP) → http://localhost:3100  (${NS_OBS}/svc/loki → container 3100)"
      kubectl port-forward -n "${NS_OBS}" svc/loki 3100:3100
      ;;
    alloy)
      echo "Forwarding Grafana Alloy → http://localhost:12345  (${NS_OBS}/svc/alloy)"
      kubectl port-forward -n "${NS_OBS}" svc/alloy 12345:12345
      ;;
    configuration)
      echo "Forwarding configuration HTTP → http://localhost:9084/swagger"
      kubectl port-forward -n "${NS_APP}" svc/configuration 9084:8080
      ;;
    messaging)
      echo "Forwarding messaging HTTP → http://localhost:9083/swagger"
      kubectl port-forward -n "${NS_APP}" svc/messaging 9083:8080
      ;;
    offer)
      echo "Forwarding offer HTTP → http://localhost:9082/swagger"
      kubectl port-forward -n "${NS_APP}" svc/offer 9082:8080
      ;;
    order)
      echo "Forwarding order HTTP → http://localhost:9081/swagger"
      kubectl port-forward -n "${NS_APP}" svc/order 9081:8080
      ;;
    frontend)
      echo "Forwarding frontend → http://localhost:9080/"
      kubectl port-forward -n "${NS_APP}" svc/frontend 9080:80
      ;;
    postgres)
      echo "Forwarding postgres → localhost:15432 (cluster port 5432)"
      kubectl port-forward -n "${NS_APP}" svc/postgres 15432:5432
      ;;
    *)
      echo "Unknown port-forward target: ${raw}" >&2
      echo "Use: grafana|logs|loki|alloy|configuration|messaging|offer|order|frontend|postgres" >&2
      exit 1
      ;;
  esac
}

cmd_services() {
  echo "=== Services: ${NS_APP} ==="
  kubectl get svc -n "${NS_APP}"
  echo
  echo "=== Services: ${NS_OBS} ==="
  if have_ns "${NS_OBS}"; then
    kubectl get svc -n "${NS_OBS}"
  else
    echo "(namespace ${NS_OBS} not found)"
  fi
}

cmd_events() {
  kubectl get events -n "${NS_APP}" --sort-by=.lastTimestamp
}

# Prints "deployment/name" or "statefulset/name" suitable for kubectl scale.
resolve_scale_target() {
  local workload="$1"

  if kubectl get deployment "${workload}" -n "${NS_APP}" &>/dev/null; then
    printf '%s\n' "deployment/${workload}"
    return 0
  fi
  if kubectl get sts "${workload}" -n "${NS_APP}" &>/dev/null; then
    printf '%s\n' "statefulset/${workload}"
    return 0
  fi
  if kubectl get pod "${workload}" -n "${NS_APP}" &>/dev/null; then
    local ok on dep
    ok=$(kubectl get pod "${workload}" -n "${NS_APP}" -o jsonpath='{.metadata.ownerReferences[?(@.controller==true)].kind}')
    on=$(kubectl get pod "${workload}" -n "${NS_APP}" -o jsonpath='{.metadata.ownerReferences[?(@.controller==true)].name}')
    if [[ "${ok}" == "ReplicaSet" && -n "${on}" ]]; then
      dep=$(kubectl get rs "${on}" -n "${NS_APP}" -o jsonpath='{.metadata.ownerReferences[?(@.kind=="Deployment")].name}')
      if [[ -n "${dep}" ]]; then
        echo "Resolved Pod/${workload} -> Deployment/${dep}" >&2
        printf '%s\n' "deployment/${dep}"
        return 0
      fi
    fi
    if [[ "${ok}" == "StatefulSet" && -n "${on}" ]]; then
      echo "Resolved Pod/${workload} -> StatefulSet/${on}" >&2
      printf '%s\n' "statefulset/${on}"
      return 0
    fi
    echo "Pod '${workload}' has no Deployment or StatefulSet controller we can scale (${ok:-no controller})." >&2
    return 1
  fi

  return 1
}

cmd_scale() {
  local workload="$1"
  local replicas="$2"

  if ! [[ "${replicas}" =~ ^[0-9]+$ ]]; then
    echo "Replicas must be a non-negative integer: ${replicas}" >&2
    exit 1
  fi

  local target
  if ! target=$(resolve_scale_target "${workload}"); then
    echo "No Deployment, StatefulSet, or Pod named '${workload}' in ${NS_APP}." >&2
    exit 1
  fi

  echo "Scaling ${target} --replicas=${replicas} (-n ${NS_APP})"
  kubectl scale "${target}" --replicas="${replicas}" -n "${NS_APP}"
}

main() {
  if [[ $# -eq 0 ]]; then
    usage >&2
    exit 1
  fi

  local cmd="$1"
  shift

  case "${cmd}" in
    -h | --help | help)
      usage
      ;;
    -pods | --pods)
      cmd_pods
      ;;
    -pods-all | --pods-all)
      cmd_pods_all
      ;;
    -deploy | --deploy)
      cmd_deploy
      ;;
    -rollout | --rollout)
      if [[ $# -ge 1 && "$1" != -* ]]; then
        cmd_rollout_one "$1"
      else
        cmd_rollout_all
      fi
      ;;
    -top | --top)
      cmd_top
      ;;
    -metrics-check | --metrics-check)
      cmd_metrics_check
      ;;
    -d | --describe)
      [[ $# -ge 1 ]] || {
        echo "Usage: $(basename "$0") -d <app>" >&2
        exit 1
      }
      cmd_describe "$1"
      ;;
    -l | --logs)
      [[ $# -ge 1 ]] || {
        echo "Usage: $(basename "$0") -l <app> [-f]" >&2
        exit 1
      }
      app="$1"
      shift
      follow=false
      if [[ "${1:-}" == "-f" ]]; then
        follow=true
      fi
      cmd_logs "${app}" "${follow}"
      ;;
    -pf | --port-forward)
      [[ $# -ge 1 ]] || {
        echo "Usage: $(basename "$0") -pf <target>" >&2
        exit 1
      }
      cmd_pf "$1"
      ;;
    -svc | --services)
      cmd_services
      ;;
    -events | --events)
      cmd_events
      ;;
    -s | --scale)
      [[ $# -ge 1 ]] || {
        echo "Usage: $(basename "$0") -s <deployment|statefulset|pod> [replicas]" >&2
        echo "       replicas defaults to 1. Example: -s order 0" >&2
        exit 1
      }
      workload="$1"
      shift
      replicas="1"
      if [[ $# -ge 1 && "$1" =~ ^[0-9]+$ ]]; then
        replicas="$1"
        shift
      else
        echo "Note: replicas not specified; using 1 (scale up one instance)." >&2
      fi
      cmd_scale "${workload}" "${replicas}"
      ;;
    *)
      echo "Unknown command: ${cmd}" >&2
      usage >&2
      exit 1
      ;;
  esac
}

main "$@"
