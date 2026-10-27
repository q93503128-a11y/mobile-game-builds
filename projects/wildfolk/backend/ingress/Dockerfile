FROM node:22-bookworm-slim
WORKDIR /app
COPY package*.json ./
RUN npm ci --omit=dev
COPY ingress/package*.json ./ingress/
RUN cd ingress && npm ci --omit=dev
COPY ingress/core.cjs ingress/server.cjs ./ingress/
COPY src/ ./src/
USER node
ENV NODE_ENV=production
CMD ["node", "ingress/server.cjs"]
