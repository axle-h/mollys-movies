#!/bin/bash
[ -f /etc/openvpn/up.sh ] && /etc/openvpn/up.sh "$@"
/usr/sbin/sockd -D