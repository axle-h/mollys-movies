# USAGE: mount an openvpn.conf file & referenced keys in /etc/openvpn/client
# REQUIRES: --device=/dev/net/tun --cap-add=NET_ADMIN
# PROVIDES: SOCKS5 server on port 1080
# NOTE: dante-server is run as a daemon so you must monitor TCP on port 1080 to ensure readiness.
FROM alpine:3
COPY sockd.sh /usr/local/bin/
RUN apk add --no-cache dante-server openvpn bash openresolv openrc curl \
    && chmod +x /usr/local/bin/sockd.sh
COPY sockd.conf /etc/
WORKDIR /etc/openvpn/client
VOLUME /etc/openvpn/client
EXPOSE 1080
CMD openvpn --config openvpn.conf --script-security 2 --up /usr/local/bin/sockd.sh