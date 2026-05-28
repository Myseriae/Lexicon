FROM node:22-alpine AS build
WORKDIR /app

# Copy dependency files first for better layer caching
COPY package.json package-lock.json ./

# Install exact locked dependencies
RUN npm ci

# Copy application source
COPY . .

# Build the app
RUN npm run build

FROM nginx:alpine AS final

# Copy built frontend
COPY --from=build /app/dist /usr/share/nginx/html

# Copy nginx config
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80