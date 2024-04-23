#!/bin/bash
set -e

npm install
npm run build

rm -rf /opt/make-movies/ui
mkdir -p /opt/make-movies/ui

cp -r .next/standalone/* /opt/make-movies/ui
cp -r .next/standalone/.next /opt/make-movies/ui/.next
cp -r public /opt/make-movies/ui/public
cp -r .next/static /opt/make-movies/ui/.next/static

chown -R alex:alex /opt/make-movies/ui

systemctl restart make-movies-ui.service
