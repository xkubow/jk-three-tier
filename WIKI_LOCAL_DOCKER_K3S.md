### Overview

This document summarizes the **local Docker** and **k3s** workflows for the jk-three-tier app, including key commands and the main problems we hit with their solutions.

---

### 1. Local Docker workflow (PostgreSQL + nginx gateway)

**Goal**: run the full three-tier app locally on Docker Desktop.

- **Env file**
  - Base file: `.env.example`
  - Create your real env:
  ```powershell
  cd c:\projects\jk-three-tier
  copy .env.example .env
  ```

  - Edit `.env` to set a strong password and keep both values in sync:
  ```env
  POSTGRES_PASSWORD=YourStrongLocalPass123
  DB_CONNECTION_STRING=Host=postgres;Port=5432;Database=jk_configurations;Username=postgres;Password=YourStrongLocalPass123;
  ```
- **Start stack**
  - Compose file is in `docker/docker-compose.yml`:
  ```powershell
  cd c:\projects\jk-three-tier\docker
  docker compose --env-file ../.env up --build
  ```

  - Services:
    - `jk-postgres` (PostgreSQL)
    - `jk-backend` (ASP.NET Core, Npgsql)
    - `jk-frontend` (Vue + nginx)
    - `jk-gateway` (nginx, `/` → FE, `/api` → BE)
- **Stop & clean up**
  ```powershell
  cd c:\projects\jk-three-tier\docker
  docker compose --env-file ../.env down        # remove containers
  docker compose --env-file ../.env down -v     # also remove volumes (DB data)
  ```
- **Test URLs**
  - Gateway: `http://localhost:8082`
  - Backend (direct): `http://localhost:8082/api/configuration/HelloWorld` (via nginx)

---

### 2. k3s workflow (WSL Ubuntu + Traefik Ingress)

**Goal**: run the same app on a local k3s cluster inside WSL and access it from Windows.

- **k3s install** (already done)
  ```bash
  curl -sfL https://get.k3s.io | sh -s - --write-kubeconfig-mode 644
  kubectl get pods -A
  ```
- **Build images on Windows**

  ```powershell
  cd c:\projects\jk-three-tier

  docker build -t jk-backend:local  .\backend
  docker build -t jk-frontend:local .\frontend
  # gateway image exists but is not used in k3s now
  docker build -t jk-gateway:local  .\gateway
  ```

- **Import images into k3s (WSL)**

  ```bash
  cd /mnt/c/projects/jk-three-tier

  docker save jk-backend:local  -o backend.tar
  docker save jk-frontend:local -o frontend.tar
  docker save jk-gateway:local  -o gateway.tar

  sudo k3s ctr images import backend.tar
  sudo k3s ctr images import frontend.tar
  sudo k3s ctr images import gateway.tar
  ```

- **Namespace**
  ```bash
  kubectl apply -f k8s/namespace.yaml
  ```
- **DB secret**
  - Edit `k8s/db-secret.example.yaml` and apply:
  ```bash
  kubectl apply -f k8s/db-secret.example.yaml
  ```

  - It defines:
  ```yaml
  POSTGRES_PASSWORD: "change-me-strong-password"
  DB_CONNECTION_STRING: "Host=postgres;Port=5432;Database=jk_configurations;Username=postgres;Password=change-me-strong-password;"
  ```
- **Deploy Postgres, backend, frontend**
  ```bash
  kubectl apply -f k8s/postgres.yaml
  kubectl apply -f k8s/backend.yaml
  kubectl apply -f k8s/frontend.yaml
  ```
- **Ingress via Traefik**
  - Ingress definition: `k8s/ingress.yaml`:
    - `/` → `frontend` Service (port 80)
    - `/api` → `backend` Service (port 8080)
  ```bash
  kubectl apply -f k8s/ingress.yaml
  ```
- **Optional: Traefik port change**
  - `k8s/traefik-config.yaml` overrides Traefik Helm values, e.g.:
  ```yaml
  apiVersion: helm.cattle.io/v1
  kind: HelmChartConfig
  metadata:
    name: traefik
    namespace: kube-system
  spec:
    valuesContent: |-
      ports:
        web:
          exposedPort: 8082
  ```

  - Apply and restart Traefik:
  ```bash
  kubectl apply -f k8s/traefik-config.yaml
  kubectl delete pod -n kube-system -l app.kubernetes.io/name=traefik
  ```
- **Access from Windows**
  1. Find the k3s/WSL node IP (inside WSL):
  ```bash
  kubectl get nodes -o wide
  kubectl get svc -n kube-system traefik
  ```
  Use the value in the `INTERNAL-IP` column for your node (e.g. `172.27.76.33`).
  2. Open the app from Windows using that IP and Traefik’s exposed port (currently 8089):
  ```text
  http://<INTERNAL-IP>:8089/
  # example: http://172.27.76.33:8089/
  ```
  Traefik receives the request on that IP:port and routes via `Ingress` to FE/BE.

- **Headlamp UI via friendly hostname**
  - Headlamp is exposed through its own ingress (`k8s/headlamp-ingress.yaml`) on the same Traefik entrypoint.
  - Add this section to `c:\Windows\System32\drivers\etc\hosts` on Windows:
  ```text
  172.27.76.33 headlamp.k3s.local
  ```
  - Then open Headlamp from Windows at:
  ```text
  http://headlamp.k3s.local:8089/
  ```

---

### 3. Common problems and solutions

#### 3.1 Postgres password mismatches (Docker & k3s)

- **Symptoms**:
  - Backend logs:
  ```text
  Npgsql.PostgresException: 28P01: password authentication failed for user "postgres"
  ```
- **Causes**:
  - `POSTGRES_PASSWORD` and `DB_CONNECTION_STRING` in `.env` (or in the k8s Secret) don’t match.
  - `.env` changed but containers were not recreated.
- **Fix (Docker)**:
  - Ensure `.env`:
  ```env
  POSTGRES_PASSWORD=...
  DB_CONNECTION_STRING=Host=postgres;Port=5432;Database=jk_configurations;Username=postgres;Password=...;
  ```

  - Then:
  ```powershell
  cd docker
  docker compose --env-file ../.env down -v
  docker compose --env-file ../.env up --build
  ```

  - Verify inside backend:
  ```powershell
  docker exec jk-backend printenv | findstr ConnectionStrings
  ```
- **Fix (k3s)**:
  - Update `k8s/db-secret.example.yaml` so both `POSTGRES_PASSWORD` and `DB_CONNECTION_STRING` use the same password.
  - Re-apply secret and restart backend:
  ```bash
  kubectl apply -f k8s/db-secret.example.yaml
  kubectl delete pod -n jk-three-tier -l app=backend
  ```

---

#### 3.2 Missing table in Postgres on k3s

- **Symptoms**:
  ```text
  SqlState: 42P01
  MessageText: relation "Configuration" does not exist
  ```
- **Cause**:
  - In Docker, schema is created by `db-init` container.
  - In k3s, we initially had no init Job/step, so the table didn’t exist.
- **Manual fix (k3s)**:

  ```bash
  # copy init.sql into the Postgres pod
  kubectl cp database/init.sql jk-three-tier/<postgres-pod>:/init.sql

  # run it
  kubectl exec -it -n jk-three-tier <postgres-pod> -- \
    psql -U postgres -d jk_configurations -f /init.sql

  # restart backend so it re-queries with schema present
  kubectl delete pod -n jk-three-tier -l app=backend
  ```

  After that, backend stops crashlooping and Traefik no longer returns 503 for `/api/...`.

---

#### 3.3 Ingress 503 from Traefik

- **Symptoms**:
  - Browser gets `503 Service Unavailable` for `/api/...`.
  - FE works or partially loads.
  - Traefik logs show no healthy upstream for the backend.
- **Cause**:
  - Backend pod is `CrashLoopBackOff` due to DB errors (or wrong port/service wiring).
- **Fix**:
  - Check pods:
  ```bash
  kubectl get pods -n jk-three-tier
  ```

  - Inspect backend logs:
  ```bash
  kubectl logs -n jk-three-tier deploy/backend
  ```

  - Fix underlying DB/schema/connection issue (see sections above), then delete backend pod to let Deployment recreate it.

---

#### 3.4 Port-forward vs NodePort vs WSL networking

- **Observations**:
  - `kubectl port-forward` run **inside WSL** binds to WSL’s loopback, not Windows’ loopback → `curl localhost:8082` works in WSL but not in Windows.
  - NodePort (e.g. `80:32401/TCP`) is reachable at `http://<WSL-IP>:32401`, not automatically at `http://localhost:32401` in Windows.
- **Practical patterns**:
  - Use Node’s IP + port from Windows:
  ```text
  http://<WSL-IP>:<nodePort>
  ```

  - Or run `kubectl port-forward` **from Windows** using exported k3s kubeconfig; then `http://localhost:<port>` works normally.

---

#### 3.5 Verifying Postgres persistence (PVC mount)

- **Check that the Postgres pod is using the PVC correctly**:

  ```bash
  kubectl describe pod -n jk-three-tier -l app=postgres
  ```

  In the output, find the `Mounts:` section for the `postgres` container. You should see something like:

  ```text
  Mounts:
    /var/lib/postgresql/data from postgres-data (rw)
  ...
  Volumes:
    postgres-data:
      Type:       PersistentVolumeClaim (a reference to a PersistentVolumeClaim in the same namespace)
      ClaimName:  postgres-data-pvc
  ```

- **Enable to use Postgres outside the wsl**

  It is needed to have TCP setting in traefik like in db-ingress-tcp.yaml. Then it's need to be applied
  ```bash
  kubectl apply -f k8s/traefik-config.yaml
  kubectl apply -f k8s/db-ingress-tcp.yaml
  kubectl delete pod -n kube-system -l app.kubernetes.io/name=traefik
  kubectl get svc -n kube-system traefik
  ```
  We need to see the port 30432 in the list. If not then it need to be applied patch.
  ```bash
  kubectl patch svc traefik -n kube-system -p
  '{"spec":{"ports":[{"name":"web","port":8089,"targetPort":"web"},{"name":"postgres","port":30432,"targetPort":"postgres"},{"name":"websecure","port":443,"targetPort":"websecure"}]}}'
  ```

- **Test that data survives pod restarts**:

  ```bash
  # 1) Restart the Postgres pod
  kubectl delete pod -n jk-three-tier -l app=postgres
  kubectl get pods -n jk-three-tier   # wait for postgres to be 1/1 Running

  # 2) Verify the data is still there
  kubectl exec -it -n jk-three-tier <postgres-pod> -- \
    psql -U postgres -d jk_configurations -c 'SELECT * FROM "Configuration";'
  ```

  If the `HelloWorld` row is still present, the PVC (`postgres-data-pvc`) is working and your DB is persistent.

---

### 4. Reference: key kubectl commands

- **List resources**
  ```bash
  kubectl get pods -n jk-three-tier
  kubectl get svc -n jk-three-tier
  kubectl get ingress -n jk-three-tier
  ```
- **Describe for debugging**
  ```bash
  kubectl describe pod -n jk-three-tier <pod>
  kubectl describe ingress -n jk-three-tier app-ingress
  ```
- **Logs**
  ```bash
  kubectl logs -n jk-three-tier deploy/backend
  kubectl logs -n jk-three-tier deploy/frontend
  ```
- **Exec into pods**
  ```bash
  kubectl exec -it -n jk-three-tier postgres-... -- psql -U postgres -d jk_configurations
  ```
- **Apply manifests**
  ```bash
  kubectl apply -f k8s/namespace.yaml
  kubectl apply -f k8s/db-secret.example.yaml
  kubectl apply -f k8s/postgres.yaml
  kubectl apply -f k8s/backend.yaml
  kubectl apply -f k8s/frontend.yaml
  kubectl apply -f k8s/ingress.yaml
  ```
