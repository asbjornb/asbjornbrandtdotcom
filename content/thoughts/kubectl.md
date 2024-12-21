# kubectl


## Check and Switch Context
```bash
kubectl config get-contexts                  # List all contexts
kubectl config use-context <context-name>    # Switch to a specific context
```

## Search for Deployments, Pods, or Services

```bash
kubectl get deployments --all-namespaces | grep <partial-name>    # Replace 'deployments' with 'pods' or 'services' as needed
```

## Get logs

```bash
kubectl logs <pod-name> -n <namespace>
```

## Describe a resource

```bash
kubectl describe deployment <deployment-name> -n <namespace>      # Use 'pod' or 'svc' instead of 'deployment' if relevant

```

## Port Forward to Access a Service Locally

```bash
kubectl port-forward -n <namespace> svc/<service-name> <local-port>:<service-port>   # Example below:
kubectl port-forward -n myteamnamespace svc/mycoolwebsite 8080:80
```

## Check Service Endpoints

```bash
kubectl get endpoints <service-name> -n <namespace>
```

If grep is unavailable (on windows), install it via Chocolatey: `choco install grep`
