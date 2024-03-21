# Open VPN Proxy

```bash
# Create the namespace
kubectl create namespace make-movies

# Create a secret for OpenVPN from local openvpn.conf
kubectl --namespace make-movies create secret generic openvpn \
    --from-file=openvpn.conf=./openvpn.conf

# Install openvpn-proxy chart from github packages
helm upgrade --namespace make-movies --install v1 oci://ghcr.io/axle-h/make-movies/openvpn-proxy --version {latest version}

# Or install/upgrade from local copy
helm upgrade --namespace make-movies --install v1 .

# To delete the release
helm --namespace make-movies delete v1
```