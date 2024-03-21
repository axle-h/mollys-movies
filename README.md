![Make Movies](https://github.com/axle-h/make-movies/actions/workflows/main.yml/badge.svg)

# Make Movies

Certified family proof (my family), movie library management on top of [Jellyfin](https://jellyfin.org) & [Transmission](https://transmissionbt.com/).

## API

[.NET API](api/README.md)

## UI

[Next.js UI](ui/README.md)

## VPN

This app works best with a vpn. You can proxy one on [k3s](https://k3s.io) via [this helm chart](helm/README.md).

## Nginx

The API + UI work well through a path routed reverse proxy. Example nginx.conf:

```
server {
	listen 8080;
	listen [::]:8080;

	location / {
        proxy_pass http://localhost:3000;
	}

	location ~ ^/(api|movie-images)/ /api {
        proxy_pass http://localhost:5000;
    }
}
```