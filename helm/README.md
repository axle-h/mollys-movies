# Molly's Movies

> Only supports single node clusters due to use of the local-path-provisioner.

```bash
# Install the nginx ingress controller (the one that supports any host routing)
helm upgrade --install ingress-nginx ingress-nginx --repo https://kubernetes.github.io/ingress-nginx --namespace ingress-nginx --create-namespace

# Install the Local Path Provisionor (not needed in k3s)
kubectl apply -f https://raw.githubusercontent.com/rancher/local-path-provisioner/v0.0.22/deploy/local-path-storage.yaml

# Create the namespace
kubectl create namespace mollys-movies

# Create a secret for OpenVPN from local openvpn.conf
kubectl --namespace mollys-movies create secret generic openvpn \
    --from-file=openvpn.conf=./openvpn.conf

# Install mollys-movies chart from gitub packages
helm upgrade --namespace mollys-movies --install v1 oci://ghcr.io/axle-h/mollys-movies/mollys-movies --version {latest version} --set plex.token=your-plex-token

# Or install/upgrade from local copy
helm dependency update
helm upgrade --namespace mollys-movies --install v1 . --set plex.token=your-plex-token

# To delete the release
helm --namespace mollys-movies delete v1
```