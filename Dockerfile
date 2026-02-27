# Stage 1: Build
FROM node:22-alpine AS builder

WORKDIR /app

# Install dependencies
COPY package.json package-lock.json ./
RUN npm ci

# Copy source and build
COPY . .

# SvelteKit requires OIDC env vars at build time for $env/static/private.
# These are placeholder values only used during the build step.
# Actual values are provided at runtime via container environment variables.
ENV OIDC_ISSUER=http://placeholder:8080/realms/risk-management
ENV OIDC_CLIENT_ID=risk-management-platform
ENV OIDC_REDIRECT_URI=http://placeholder:3000/auth/callback
ENV OIDC_POST_LOGOUT_REDIRECT_URI=http://placeholder:3000/
ENV OIDC_SCOPE="openid profile email"
ENV OIDC_ROLES_CLAIM_PATH=realm_access.roles

RUN npm run build

# Stage 2: Production
FROM node:22-alpine AS production

WORKDIR /app

# Install only production dependencies
COPY package.json package-lock.json ./
RUN npm ci --omit=dev

# Copy the built application from builder stage
COPY --from=builder /app/build ./build

# Create directory for SQLite database (local container storage)
# Azure Files (SMB) does not support POSIX file locking required by SQLite.
# Data is ephemeral until migrating to a managed database.
RUN mkdir -p /app/data

# Environment variables
ENV NODE_ENV=production
ENV PORT=3000
ENV HOST=0.0.0.0
ENV DATABASE_PATH=/app/data/data.db
ENV ORIGIN=http://localhost:3000

EXPOSE 3000

CMD ["node", "build"]
