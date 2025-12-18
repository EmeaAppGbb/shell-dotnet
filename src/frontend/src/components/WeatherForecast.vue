<script setup lang="ts">
import { ref, onMounted } from 'vue'
import type { WeatherForecast } from '@/types/weather'
import { getWeatherForecast } from '@/services/api'

const forecasts = ref<WeatherForecast[]>([])
const loading = ref(true)
const error = ref<string | null>(null)

onMounted(async () => {
  try {
    forecasts.value = await getWeatherForecast()
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Failed to load forecast'
  } finally {
    loading.value = false
  }
})

function formatDate(dateStr: string): string {
  const date = new Date(dateStr)
  return date.toLocaleDateString('en-US', {
    weekday: 'short',
    month: 'short',
    day: 'numeric',
  })
}

function getWeatherEmoji(summary: string | null): string {
  if (!summary) return '🌤️'
  const s = summary.toLowerCase()
  if (s.includes('freezing') || s.includes('bracing')) return '🥶'
  if (s.includes('chilly') || s.includes('cool')) return '❄️'
  if (s.includes('mild')) return '🌤️'
  if (s.includes('warm') || s.includes('balmy')) return '☀️'
  if (s.includes('hot') || s.includes('sweltering') || s.includes('scorching')) return '🔥'
  return '🌤️'
}
</script>

<template>
  <div class="weather-forecast">
    <div class="header-row">
      <div>
        <p class="eyebrow">Upcoming</p>
        <h3>Weather forecast</h3>
      </div>
    </div>

    <div v-if="loading" class="loading">Loading forecast...</div>

    <div v-else-if="error" class="error">{{ error }}</div>

    <div v-else class="forecast-grid">
      <div v-for="forecast in forecasts" :key="forecast.date" class="forecast-card">
        <div class="forecast-emoji">{{ getWeatherEmoji(forecast.summary) }}</div>
        <div class="forecast-date">{{ formatDate(forecast.date) }}</div>
        <div class="forecast-temp">
          <span class="temp-c">{{ forecast.temperatureC }}°C</span>
          <span class="temp-f">{{ forecast.temperatureF }}°F</span>
        </div>
        <div class="forecast-summary">{{ forecast.summary }}</div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.weather-forecast {
  margin-top: 1rem;
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.header-row h3 {
  margin: 0;
  color: var(--color-heading);
}

.eyebrow {
  text-transform: uppercase;
  letter-spacing: 0.08em;
  font-size: 0.75rem;
  color: var(--color-text-muted);
  font-weight: 700;
}

.loading,
.error {
  text-align: center;
  padding: 2rem;
}

.error {
  color: #ef4444;
}

.forecast-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
  gap: 1rem;
  max-width: 900px;
  margin: 0 auto;
}

.forecast-card {
  background: var(--color-surface-muted);
  border-radius: 12px;
  padding: 1.25rem;
  text-align: center;
  transition: transform 0.18s ease, box-shadow 0.18s ease;
  border: 1px solid var(--color-border);
  box-shadow: 0 8px 22px rgba(15, 23, 42, 0.05);
}

.forecast-card:hover {
  transform: translateY(-3px);
  box-shadow: 0 12px 28px rgba(15, 23, 42, 0.08);
}

.forecast-emoji {
  font-size: 2.5rem;
  margin-bottom: 0.5rem;
}

.forecast-date {
  font-weight: 600;
  color: var(--color-heading);
  margin-bottom: 0.5rem;
}

.forecast-temp {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
  margin-bottom: 0.5rem;
}

.temp-c {
  font-size: 1.5rem;
  font-weight: bold;
  color: var(--color-text);
}

.temp-f {
  font-size: 0.875rem;
  color: var(--color-text-muted, #888);
}

.forecast-summary {
  font-size: 0.875rem;
  color: var(--color-text);
  font-style: italic;
}
</style>
