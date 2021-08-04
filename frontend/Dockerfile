FROM node:lts as build-stage
WORKDIR /app
COPY package.json /app/
COPY yarn.lock /app/
RUN yarn install --frozen-lockfile
COPY ./ /app/
RUN yarn build

FROM nginx:latest
COPY default.conf /etc/nginx/conf.d/
COPY --from=build-stage /app/dist/ /usr/share/nginx/html
EXPOSE 5000:5000
