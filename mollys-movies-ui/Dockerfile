FROM node:16 AS build
WORKDIR /app
COPY package.json package-lock.json ./
RUN npm install
COPY . .
RUN npm run build:prod

FROM nginx:1
COPY nginx.conf /etc/nginx/nginx.conf
COPY --from=build /app/dist/mollys-movies-ui /usr/share/nginx/html
