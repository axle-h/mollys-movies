# Make Movies UI

No frills [Next.js](https://nextjs.org) + [Chakra](https://chakra-ui.com) based UI for movie library maintenance.

## Development

Works well with a mock api provided in `./mock-api`.

Then:

```bash
npm run dev
```

Open [http://localhost:3000](http://localhost:3000) with your browser.

## Deploy

For simplicity, I run this on Ubuntu via systemd.

1. Build and copy an export to a central place (note there is some juggling here, all documented via Next.js).

    ```shell
    npm install
    npm run build

    mkdir -p /opt/make-movies/ui

    cp -r .next/standalone/* /opt/make-movies/ui
    cp -r .next/standalone/.next /opt/make-movies/ui/.next
    cp -r public /opt/make-movies/ui/public
    cp -r .next/static /opt/make-movies/ui/.next/static
    ```
2. Create systemd unit in `/etc/systemd/system/make-movies-ui.service`:

    ```ini
    [Unit]
    Description=Make movies UI
    Wants=network-online.target
    After=network-online.target

    [Service]
    Type=simple
    Restart=on-failure
    WorkingDirectory=/opt/make-movies/ui
    ExecStart=node /opt/make-movies/ui/server.js

    [Install]
    WantedBy=default.target
    ```
3. Reload systemd, start and enable the service:
    ```shell
    sudo systemctl daemon-reload
    sudo systemctl enable make-movies-ui
    sudo systemctl start make-movies-ui
    ```
4. Browse to [http://localhost:3000](http://localhost:3000)