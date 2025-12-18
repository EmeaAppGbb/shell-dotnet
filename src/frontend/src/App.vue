<script setup lang="ts">
import { RouterLink, RouterView } from 'vue-router'
</script>

<template>
  <div class="app-shell">
    <header class="topbar">
      <div class="brand">
        <div class="brand-mark">°C</div>
        <div>
          <p class="brand-name">ClimaSphere</p>
          <p class="brand-tagline">Weather insights & control</p>
        </div>
      </div>

      <nav class="nav-links">
        <RouterLink to="/">Overview</RouterLink>
        <RouterLink to="/about">About</RouterLink>
      </nav>

      <a class="primary-action" href="mailto:ops@climasphere.app">Contact</a>
    </header>

    <main class="page-content">
      <RouterView />
    </main>
  </div>
</template>

<style scoped>
.app-shell {
  min-height: 100vh;
  color: var(--color-text);
}

.topbar {
  position: sticky;
  top: 0;
  z-index: 10;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1.5rem;
  padding: 1.25rem 1.5rem;
  margin-bottom: 1.5rem;
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: 14px;
  box-shadow: 0 12px 35px rgba(15, 23, 42, 0.08);
  backdrop-filter: blur(8px);
}

.brand {
  display: flex;
  align-items: center;
  gap: 0.8rem;
}

.brand-mark {
  width: 44px;
  height: 44px;
  border-radius: 12px;
  background: linear-gradient(135deg, var(--color-accent), var(--color-accent-strong));
  color: #fff;
  display: grid;
  place-items: center;
  font-weight: 700;
  letter-spacing: -0.03em;
}

.brand-name {
  font-weight: 700;
  margin: 0;
  color: var(--color-heading);
}

.brand-tagline {
  margin: 0;
  color: var(--color-text-muted);
  font-size: 0.9rem;
}

.nav-links {
  display: flex;
  align-items: center;
  gap: 1rem;
  font-weight: 600;
}

.nav-links a {
  color: var(--color-text);
  padding: 0.4rem 0.75rem;
  border-radius: 10px;
  border: 1px solid transparent;
  transition: all 0.18s ease;
}

.nav-links a.router-link-active {
  background: var(--color-surface-muted);
  border-color: var(--color-border);
  color: var(--color-heading);
}

.nav-links a:hover {
  color: var(--color-heading);
  background: var(--color-surface-muted);
}

.primary-action {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  padding: 0.55rem 1.1rem;
  border-radius: 12px;
  background: linear-gradient(135deg, var(--color-accent), var(--color-accent-strong));
  color: #fff;
  font-weight: 700;
  letter-spacing: -0.01em;
  border: 1px solid transparent;
  transition: transform 0.15s ease, box-shadow 0.15s ease;
  box-shadow: 0 10px 30px rgba(37, 99, 235, 0.25);
}

.primary-action:hover {
  transform: translateY(-1px);
  box-shadow: 0 14px 30px rgba(37, 99, 235, 0.32);
}

.page-content {
  padding: 0 0.5rem 2rem;
}

@media (max-width: 768px) {
  .topbar {
    flex-direction: column;
    align-items: flex-start;
  }

  .nav-links {
    width: 100%;
    justify-content: flex-start;
  }

  .primary-action {
    width: 100%;
    text-align: center;
  }
}
</style>
