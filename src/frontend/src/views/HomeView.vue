<script setup lang="ts">
import WeatherForecast from '../components/WeatherForecast.vue'
import TemperatureManager from '../components/TemperatureManager.vue'
</script>

<template>
  <section class="hero surface">
    <div class="hero-text">
      <p class="eyebrow">Operations dashboard</p>
      <h1>Weather intelligence for your sites</h1>
      <p class="lede">
        Monitor forecasts and manage on-site sensors with realtime temperature data. Everything you
        need to keep environments healthy and predictable.
      </p>
      <div class="actions">
        <a class="btn primary" href="#measurements">Record temperature</a>
        <a class="btn ghost" href="#forecast">View forecast</a>
      </div>
      <div class="stats">
        <div class="stat-card">
          <p class="stat-label">Active sensors</p>
          <p class="stat-value">24</p>
        </div>
        <div class="stat-card">
          <p class="stat-label">Cities covered</p>
          <p class="stat-value">12</p>
        </div>
        <div class="stat-card">
          <p class="stat-label">Today’s sync</p>
          <p class="stat-value">99.4%</p>
        </div>
      </div>
    </div>
    <div class="hero-panel surface">
      <h3>Live operations</h3>
      <ul>
        <li>
          <span class="dot" aria-hidden="true"></span>
          Five-minute sensor polling across all regions
        </li>
        <li>
          <span class="dot" aria-hidden="true"></span>
          Anomaly alerts routed to Ops & Facilities
        </li>
        <li>
          <span class="dot" aria-hidden="true"></span>
          Multi-region API with intelligent routing
        </li>
      </ul>
      <div class="panel-footer">
        <p>Uptime this week</p>
        <strong>99.95%</strong>
      </div>
    </div>
  </section>

  <section id="forecast" class="panel surface">
    <div class="panel-head">
      <div>
        <p class="eyebrow">Planning</p>
        <h2>Forecast outlook</h2>
        <p class="lede">See how the next few days shape up before dispatching teams.</p>
      </div>
    </div>
    <WeatherForecast />
  </section>

  <section id="measurements" class="panel surface">
    <div class="panel-head">
      <div>
        <p class="eyebrow">Data capture</p>
        <h2>Temperature log</h2>
        <p class="lede">Add, edit, and audit measurements coming from the field.</p>
      </div>
    </div>
    <TemperatureManager />
  </section>
</template>

<style scoped>
.hero {
  display: grid;
  grid-template-columns: 1.5fr 1fr;
  gap: 1.5rem;
  padding: 1.75rem;
  margin-bottom: 1.5rem;
}

.hero-text h1 {
  font-size: 2.1rem;
  line-height: 1.2;
  margin: 0.3rem 0 0.6rem;
  color: var(--color-heading);
}

.eyebrow {
  text-transform: uppercase;
  letter-spacing: 0.08em;
  font-size: 0.75rem;
  color: var(--color-text-muted);
  font-weight: 700;
}

.lede {
  color: var(--color-text-muted);
  max-width: 640px;
}

.actions {
  display: flex;
  gap: 0.75rem;
  margin: 1.25rem 0;
  flex-wrap: wrap;
}

.btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  padding: 0.65rem 1rem;
  border-radius: 12px;
  font-weight: 700;
  border: 1px solid transparent;
  cursor: pointer;
}

.btn.primary {
  background: linear-gradient(135deg, var(--color-accent), var(--color-accent-strong));
  color: #fff;
  box-shadow: 0 12px 32px rgba(37, 99, 235, 0.25);
}

.btn.ghost {
  background: var(--color-surface-muted);
  color: var(--color-heading);
  border-color: var(--color-border);
}

.stats {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
  gap: 0.75rem;
  margin-top: 1rem;
}

.stat-card {
  padding: 0.85rem 1rem;
  border: 1px solid var(--color-border);
  border-radius: 12px;
  background: var(--color-surface-muted);
}

.stat-label {
  margin: 0;
  color: var(--color-text-muted);
  font-size: 0.9rem;
}

.stat-value {
  margin: 0.1rem 0 0;
  font-weight: 800;
  font-size: 1.5rem;
  color: var(--color-heading);
}

.hero-panel {
  padding: 1.5rem;
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.hero-panel h3 {
  margin: 0;
  color: var(--color-heading);
}

.hero-panel ul {
  list-style: none;
  padding: 0;
  margin: 0;
  display: flex;
  flex-direction: column;
  gap: 0.65rem;
  color: var(--color-text);
}

.dot {
  width: 10px;
  height: 10px;
  border-radius: 50%;
  background: var(--color-success);
  display: inline-block;
  margin-right: 0.4rem;
}

.panel-footer {
  display: flex;
  align-items: center;
  justify-content: space-between;
  border-top: 1px solid var(--color-border);
  padding-top: 0.75rem;
  color: var(--color-text-muted);
}

.panel {
  padding: 1.5rem;
  margin-bottom: 1rem;
}

.panel-head h2 {
  margin: 0.25rem 0;
  color: var(--color-heading);
}

@media (max-width: 960px) {
  .hero {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 640px) {
  .hero,
  .panel {
    padding: 1.1rem;
  }

  .stat-card {
    text-align: center;
  }
}
</style>
