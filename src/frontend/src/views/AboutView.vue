<template>
  <section class="about surface">
    <p class="eyebrow">About</p>
    <h1>Operational weather made simple</h1>
    <p class="lede">
      We help teams monitor forecasts, capture measurements, and react to environmental change with
      confidence. ClimaSphere blends sensor data with reliable forecasting so operations, facilities,
      and field teams stay aligned.
    </p>

    <div class="pill-grid">
      <div class="pill">
        <strong>Resilient platform</strong>
        <p>Multi-region API routing with automatic failover and live diagnostics.</p>
      </div>
      <div class="pill">
        <strong>Actionable signals</strong>
        <p>Sensor anomalies and forecast shifts are routed to the right owners instantly.</p>
      </div>
      <div class="pill">
        <strong>Human-friendly UX</strong>
        <p>Clean workflows for logging data, reviewing trends, and collaborating on responses.</p>
      </div>
    </div>

    <div class="meta-grid">
      <div class="meta-card">
        <p class="label">Availability</p>
        <h3>99.95%</h3>
        <p class="muted">Backed by multi-region deployment and proactive alerting.</p>
      </div>
      <div class="meta-card">
        <p class="label">Data latency</p>
        <h3>&lt; 400 ms</h3>
        <p class="muted">From sensor ingestion to live dashboards worldwide.</p>
      </div>
      <div class="meta-card">
        <p class="label">Support</p>
        <h3>24/7</h3>
        <p class="muted">Real humans for operations, facilities, and field engineering teams.</p>
      </div>
    </div>
  </section>
</template>

<style scoped>
.about {
  padding: 1.5rem;
  margin-bottom: 1rem;
}

h1 {
  margin: 0.3rem 0 0.6rem;
  color: var(--color-heading);
  font-size: 2rem;
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
  max-width: 720px;
  margin-bottom: 1.5rem;
}

.pill-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
  gap: 0.75rem;
  margin-bottom: 1.5rem;
}

.pill {
  padding: 1rem;
  border-radius: 12px;
  border: 1px solid var(--color-border);
  background: var(--color-surface-muted);
}

.pill strong {
  display: block;
  margin-bottom: 0.35rem;
  color: var(--color-heading);
}

.pill p {
  margin: 0;
  color: var(--color-text);
}

.meta-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1rem;
}

.meta-card {
  padding: 1rem;
  border-radius: 12px;
  border: 1px solid var(--color-border);
  background: var(--color-surface);
  box-shadow: 0 8px 22px rgba(15, 23, 42, 0.04);
}

.label {
  text-transform: uppercase;
  letter-spacing: 0.08em;
  font-size: 0.75rem;
  color: var(--color-text-muted);
  margin: 0 0 0.3rem;
}

.muted {
  color: var(--color-text-muted);
  margin: 0;
}

h3 {
  margin: 0 0 0.3rem;
  color: var(--color-heading);
}

@media (max-width: 640px) {
  .about {
    padding: 1.1rem;
  }
}
</style>
