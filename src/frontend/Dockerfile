FROM node:22-alpine AS builder
WORKDIR /app
COPY package.json package-lock.json ./
RUN npm ci
COPY . .
RUN npm run build

FROM node:22-alpine AS runner
WORKDIR /app
ENV NODE_ENV=production
COPY --from=builder /app/build ./build
COPY --from=builder /app/package.json ./
COPY --from=builder /app/package-lock.json ./
COPY --from=builder /app/node_modules ./node_modules
COPY --from=builder /app/src/lib/server/db ./src/lib/server/db
COPY --from=builder /app/src/lib/server/services ./src/lib/server/services
COPY --from=builder /app/scripts ./scripts
COPY --from=builder /app/tsconfig.json ./
EXPOSE 3000
CMD ["sh", "-c", "npx drizzle-kit push && npx tsx scripts/seed.ts && node build/index.js"]
