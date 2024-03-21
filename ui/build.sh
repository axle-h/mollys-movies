#!/bin/bash
set -e

npm install
npm run build

sudo rm -rf /opt/make-movies/ui
sudo mkdir -p /opt/make-movies/ui


sudo cp -r .next/standalone/* /opt/make-movies/ui
sudo cp -r .next/standalone/.next /opt/make-movies/ui/.next
sudo cp -r public /opt/make-movies/ui/public
sudo cp -r .next/static /opt/make-movies/ui/.next/static

sudo chown -R alex:alex /opt/make-movies/ui

sudo systemctl restart make-movies-ui.service
