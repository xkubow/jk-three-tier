#!/usr/bin/env bash
set -euo pipefail

NS_APP="jk-three-tier"
NS_OBS="observability"
APPLY_YAML=0
WAIT_ROLLOUT=0
ROLLOUT_TIMEOUT="120s"

DEFAULT_RESTART_TARGETS=(configuration messaging offer order)
SUPPORTED_NAMESPACES=("$NS_APP" "$NS_OBS")

declare -a REQUESTED_TARGETS=()
declare -a RESOLVED_TARGETS=()
declare -a WATCH_NAMESPACES=()
declare -A SEEN_TARGETS=()
declare -A SEEN_WATCH_NAMESPACES=()
declare -A SELECTED_DEPLOYABLES=()

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
IMAGE_TAG_FILE="$REPO_ROOT/.backend-image-tag"
IMAGE_TAG="local"
TEMP_MANIFEST_DIR=""

cleanup() {
  if [[ -n "$TEMP_MANIFEST_DIR" && -d "$TEMP_MANIFEST_DIR" ]]; then
    rm -rf "$TEMP_MANIFEST_DIR"
  fi
}

trap cleanup EXIT

usage() {
  cat <<EOF
Usage: $(basename "$0") [-ay] [--wait | -w] [-t target[,target...]] [target ...]

Imports selected jk-*-<tag>.tar images from the repo root into k3s (tag from
.backend-image-tag, colons in the tag become hyphens in the filename), optionally applies
selected backend manifests rendered with the latest built image tag, updates backend
Deployments to the latest built image tag when manifests are not applied, rollout
restarts the requested workloads, optionally waits for rollout completion, then
watches Pods.

If no target is provided, defaults to: ${DEFAULT_RESTART_TARGETS[*]}

Targets:
  configuration messaging offer order frontend postgres alloy
  all            Restart every Deployment/DaemonSet/StatefulSet in ${NS_APP} and ${NS_OBS}
  <pod-name>     Resolve the Pod to its owning Deployment/DaemonSet/StatefulSet and restart that

Options:
  -t, --target  Target selector; may be repeated and may contain comma-separated values
  -ay           kubectl apply selected backend manifests from k8s/*.yaml
  --wait, -w    kubectl rollout status per restarted workload (timeout ${ROLLOUT_TIMEOUT} each)
  -h, --help    Show this help and examples
EOF
}

show_help() {
  usage
  echo
  echo "Examples:"
  echo "  $(basename "$0")"
  echo "  $(basename "$0") alloy"
  echo "  $(basename "$0") frontend postgres"
  echo "  $(basename "$0") -t alloy"
  echo "  $(basename "$0") -t configuration,order"
  echo "  $(basename "$0") --target alloy --target frontend"
  echo "  $(basename "$0") all --wait"
  echo "  $(basename "$0") configuration-6d8bf9f6c9-abcde"
  echo
  echo "Direct kubectl equivalents:"
  echo "  kubectl rollout restart daemonset/alloy -n ${NS_OBS}"
  echo "  kubectl get pods -n ${NS_OBS} -w"
}

have_ns() {
  kubectl get ns "$1" &>/dev/null
}

trim_whitespace() {
  local value="$1"
  value="${value#"${value%%[![:space:]]*}"}"
  value="${value%"${value##*[![:space:]]}"}"
  printf '%s\n' "$value"
}

load_image_tag() {
  local tag=""

  if [[ -f "$IMAGE_TAG_FILE" ]]; then
    tag="$(<"$IMAGE_TAG_FILE")"
    tag="$(trim_whitespace "$tag")"
  fi

  if [[ -n "$tag" ]]; then
    IMAGE_TAG="$tag"
    return 0
  fi

  echo "No image tag file found at $IMAGE_TAG_FILE; defaulting backend images to tag '$IMAGE_TAG'." >&2
}

ensure_temp_manifest_dir() {
  if [[ -z "$TEMP_MANIFEST_DIR" ]]; then
    TEMP_MANIFEST_DIR=$(mktemp -d)
  fi
}

append_target_arg() {
  local raw="$1"
  local item trimmed
  local -a items=()

  IFS=',' read -r -a items <<<"$raw"

  for item in "${items[@]}"; do
    trimmed=$(trim_whitespace "$item")
    if [[ -z "$trimmed" ]]; then
      echo "Empty target in list '$raw'." >&2
      exit 1
    fi
    REQUESTED_TARGETS+=("$trimmed")
  done
}

is_deployable_app() {
  case "$1" in
    configuration|messaging|offer|order) return 0 ;;
    *) return 1 ;;
  esac
}

image_tar_suffix() {
  # Match build-backend-all.bat: ":" in tag -> "-" in archive name
  printf '%s\n' "${IMAGE_TAG//:/-}"
}

image_tar_for() {
  local suffix
  suffix="$(image_tar_suffix)"
  case "$1" in
    configuration) printf '%s\n' "jk-configuration-${suffix}.tar" ;;
    messaging) printf '%s\n' "jk-messaging-${suffix}.tar" ;;
    offer) printf '%s\n' "jk-offer-${suffix}.tar" ;;
    order) printf '%s\n' "jk-order-${suffix}.tar" ;;
    *) return 1 ;;
  esac
}

image_ref_for() {
  case "$1" in
    configuration) printf '%s\n' "jk-configuration:${IMAGE_TAG}" ;;
    messaging) printf '%s\n' "jk-messaging:${IMAGE_TAG}" ;;
    offer) printf '%s\n' "jk-offer:${IMAGE_TAG}" ;;
    order) printf '%s\n' "jk-order:${IMAGE_TAG}" ;;
    *) return 1 ;;
  esac
}

manifest_for() {
  case "$1" in
    configuration|messaging|offer|order) printf '%s\n' "$1" ;;
    *) return 1 ;;
  esac
}

render_manifest_for() {
  local app="$1"
  local manifest source_manifest rendered_manifest image_ref

  manifest=$(manifest_for "$app")
  source_manifest="$REPO_ROOT/k8s/${manifest}.yaml"
  image_ref=$(image_ref_for "$app")

  ensure_temp_manifest_dir
  rendered_manifest="$TEMP_MANIFEST_DIR/${manifest}.yaml"

  sed -E "s|(^[[:space:]]*image:[[:space:]]*)jk-${app}:[^[:space:]]+|\\1${image_ref}|g" \
    "$source_manifest" > "$rendered_manifest"

  printf '%s\n' "$rendered_manifest"
}

add_watch_namespace() {
  local ns="$1"
  if [[ -z "${SEEN_WATCH_NAMESPACES[$ns]+x}" ]]; then
    SEEN_WATCH_NAMESPACES["$ns"]=1
    WATCH_NAMESPACES+=("$ns")
  fi
}

add_resolved_target() {
  local ns="$1"
  local target="$2"
  local key="${ns}|${target}"
  local target_name="${target#*/}"

  if [[ -n "${SEEN_TARGETS[$key]+x}" ]]; then
    return 0
  fi

  SEEN_TARGETS["$key"]=1
  RESOLVED_TARGETS+=("$key")
  add_watch_namespace "$ns"

  if [[ "$ns" == "$NS_APP" ]] && is_deployable_app "$target_name"; then
    SELECTED_DEPLOYABLES["$target_name"]=1
  fi
}

kind_to_resource() {
  case "$1" in
    Deployment) printf '%s\n' "deployment" ;;
    DaemonSet) printf '%s\n' "daemonset" ;;
    StatefulSet) printf '%s\n' "statefulset" ;;
    *) return 1 ;;
  esac
}

resolve_pod_owner() {
  local ns="$1"
  local pod="$2"
  local owner_kind owner_name resource deployment_name

  owner_kind=$(kubectl get pod "$pod" -n "$ns" -o jsonpath='{.metadata.ownerReferences[?(@.controller==true)].kind}')
  owner_name=$(kubectl get pod "$pod" -n "$ns" -o jsonpath='{.metadata.ownerReferences[?(@.controller==true)].name}')

  if [[ "$owner_kind" == "ReplicaSet" && -n "$owner_name" ]]; then
    deployment_name=$(kubectl get rs "$owner_name" -n "$ns" -o jsonpath='{.metadata.ownerReferences[?(@.kind=="Deployment")].name}')
    if [[ -n "$deployment_name" ]]; then
      echo "Resolved pod/$pod -> deployment/$deployment_name (-n $ns)" >&2
      printf '%s\n' "deployment/$deployment_name"
      return 0
    fi
  fi

  if resource=$(kind_to_resource "$owner_kind" 2>/dev/null); then
    echo "Resolved pod/$pod -> ${resource}/$owner_name (-n $ns)" >&2
    printf '%s\n' "${resource}/$owner_name"
    return 0
  fi

  echo "Pod '$pod' in namespace '$ns' has no supported controller for rollout restart (${owner_kind:-no controller})." >&2
  return 1
}

resolve_restart_target() {
  local workload="$1"
  local ns resource resolved

  for ns in "${SUPPORTED_NAMESPACES[@]}"; do
    have_ns "$ns" || continue

    for resource in deployment daemonset statefulset; do
      if kubectl get "$resource" "$workload" -n "$ns" &>/dev/null; then
        printf '%s|%s/%s\n' "$ns" "$resource" "$workload"
        return 0
      fi
    done

    if kubectl get pod "$workload" -n "$ns" &>/dev/null; then
      resolved=$(resolve_pod_owner "$ns" "$workload") || return 1
      printf '%s|%s\n' "$ns" "$resolved"
      return 0
    fi
  done

  return 1
}

collect_all_restart_targets() {
  local ns line resource name

  for ns in "${SUPPORTED_NAMESPACES[@]}"; do
    have_ns "$ns" || continue
    while IFS= read -r line; do
      [[ -n "$line" ]] || continue
      resource="${line%%/*}"
      resource="${resource%%.*}"
      name="${line#*/}"
      printf '%s|%s/%s\n' "$ns" "$resource" "$name"
    done < <(kubectl get deploy,daemonset,statefulset -n "$ns" -o name 2>/dev/null || true)
  done
}

resolve_requested_targets() {
  local raw resolved ns target

  if [[ "${#REQUESTED_TARGETS[@]}" -eq 0 ]]; then
    REQUESTED_TARGETS=("${DEFAULT_RESTART_TARGETS[@]}")
  fi

  for raw in "${REQUESTED_TARGETS[@]}"; do
    if [[ "$raw" == "all" ]]; then
      while IFS='|' read -r ns target; do
        [[ -n "$ns" && -n "$target" ]] || continue
        add_resolved_target "$ns" "$target"
      done < <(collect_all_restart_targets)
      continue
    fi

    if ! resolved=$(resolve_restart_target "$raw"); then
      echo "No deployment, daemonset, statefulset, or pod named '$raw' found in ${SUPPORTED_NAMESPACES[*]}." >&2
      exit 1
    fi

    IFS='|' read -r ns target <<<"$resolved"
    add_resolved_target "$ns" "$target"
  done

  if [[ "${#RESOLVED_TARGETS[@]}" -eq 0 ]]; then
    echo "No restartable workloads were resolved." >&2
    exit 1
  fi
}

import_selected_images() {
  local app tar imported_any=0

  echo "Repo root: $REPO_ROOT"
  echo "Using backend image tag: $IMAGE_TAG"
  echo "Importing images into k3s (requires sudo)..."
  echo

  for app in "${DEFAULT_RESTART_TARGETS[@]}"; do
    if [[ -z "${SELECTED_DEPLOYABLES[$app]+x}" ]]; then
      continue
    fi
    tar=$(image_tar_for "$app")
    echo "Importing $tar..."
    sudo k3s ctr images import "$REPO_ROOT/$tar"
    imported_any=1
  done

  echo
  if [[ "$imported_any" -eq 1 ]]; then
    echo "Images imported."
  else
    echo "No local image archives selected for import."
  fi
}

apply_selected_manifests() {
  local app manifest rendered_manifest applied_any=0

  [[ "$APPLY_YAML" -eq 1 ]] || return 0

  echo
  echo "Applying backend manifests..."
  for app in "${DEFAULT_RESTART_TARGETS[@]}"; do
    if [[ -z "${SELECTED_DEPLOYABLES[$app]+x}" ]]; then
      continue
    fi
    manifest=$(manifest_for "$app")
    rendered_manifest=$(render_manifest_for "$app")
    echo "Applying rendered k8s/${manifest}.yaml with image $(image_ref_for "$app")..."
    kubectl apply -f "$rendered_manifest"
    applied_any=1
  done

  if [[ "$applied_any" -eq 0 ]]; then
    echo "No backend manifests selected for apply."
  fi
}

update_selected_images() {
  local app image updated_any=0

  [[ "$APPLY_YAML" -eq 1 ]] && return 0

  echo
  echo "Updating backend deployment images..."
  for app in "${DEFAULT_RESTART_TARGETS[@]}"; do
    if [[ -z "${SELECTED_DEPLOYABLES[$app]+x}" ]]; then
      continue
    fi

    image=$(image_ref_for "$app")
    echo "Setting deployment/$app image to $image..."
    kubectl set image "deployment/$app" "$app=$image" -n "$NS_APP"
    updated_any=1
  done

  if [[ "$updated_any" -eq 0 ]]; then
    echo "No backend deployment images selected for update."
  fi
}

rollout_restart_targets() {
  local entry ns target

  echo
  echo "Rolling out restarts..."
  for entry in "${RESOLVED_TARGETS[@]}"; do
    IFS='|' read -r ns target <<<"$entry"
    echo "  kubectl rollout restart ${target} -n ${ns}"
    kubectl rollout restart "$target" -n "$ns"
  done
}

wait_for_rollouts() {
  local entry ns target

  [[ "$WAIT_ROLLOUT" -eq 1 ]] || return 0

  echo
  echo "Waiting for rollouts (${ROLLOUT_TIMEOUT} each)..."
  for entry in "${RESOLVED_TARGETS[@]}"; do
    IFS='|' read -r ns target <<<"$entry"
    echo "  kubectl rollout status ${target} -n ${ns} --timeout=${ROLLOUT_TIMEOUT}"
    kubectl rollout status "$target" -n "$ns" --timeout="$ROLLOUT_TIMEOUT"
  done
}

watch_pods() {
  local ns

  echo
  if [[ "${#WATCH_NAMESPACES[@]}" -eq 1 ]]; then
    ns="${WATCH_NAMESPACES[0]}"
    echo "Watching pods in ${ns} (Ctrl+C to stop)..."
    kubectl get pods -n "$ns" -w
    return 0
  fi

  echo "Watching pods in all namespaces (Ctrl+C to stop)..."
  kubectl get pods -A -w
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    -t|--target)
      if [[ $# -lt 2 ]]; then
        echo "Option $1 requires a target value." >&2
        usage >&2
        exit 1
      fi
      append_target_arg "$2"
      shift
      ;;
    --target=*)
      append_target_arg "${1#*=}"
      ;;
    -ay) APPLY_YAML=1 ;;
    --wait|-w) WAIT_ROLLOUT=1 ;;
    -h|--help|/h)
      show_help
      exit 0
      ;;
    -*)
      echo "Unknown option: $1" >&2
      usage >&2
      exit 1
      ;;
    *)
      append_target_arg "$1"
      ;;
  esac
  shift
done

cd "$REPO_ROOT"

load_image_tag
resolve_requested_targets
import_selected_images
apply_selected_manifests
update_selected_images
rollout_restart_targets
wait_for_rollouts
watch_pods
