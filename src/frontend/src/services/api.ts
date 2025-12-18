import type {
  WeatherForecast,
  TemperatureMeasurement,
  CreateTemperatureMeasurement,
  UpdateTemperatureMeasurement,
} from '@/types/weather'

// Prefer a build-time injected backend URL (set VITE_BACKEND_URL) so prod calls do not loop back to the SPA host.
const API_BASE = (import.meta.env.VITE_BACKEND_URL || '').replace(/\/$/, '')

// Weather Forecast API
export async function getWeatherForecast(): Promise<WeatherForecast[]> {
  const response = await fetch(`${API_BASE}/weatherforecast`)
  if (!response.ok) {
    throw new Error('Failed to fetch weather forecast')
  }
  return response.json()
}

// Temperature Measurements CRUD API
export async function getTemperatures(): Promise<TemperatureMeasurement[]> {
  const response = await fetch(`${API_BASE}/api/temperatures`)
  if (!response.ok) {
    throw new Error('Failed to fetch temperatures')
  }
  return response.json()
}

export async function getTemperatureById(id: string): Promise<TemperatureMeasurement> {
  const response = await fetch(`${API_BASE}/api/temperatures/${id}`)
  if (!response.ok) {
    throw new Error('Failed to fetch temperature')
  }
  return response.json()
}

export async function createTemperature(
  data: CreateTemperatureMeasurement,
): Promise<TemperatureMeasurement> {
  const response = await fetch(`${API_BASE}/api/temperatures`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(data),
  })
  if (!response.ok) {
    throw new Error('Failed to create temperature')
  }
  return response.json()
}

export async function updateTemperature(
  id: string,
  data: UpdateTemperatureMeasurement,
): Promise<TemperatureMeasurement> {
  const response = await fetch(`${API_BASE}/api/temperatures/${id}`, {
    method: 'PUT',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(data),
  })
  if (!response.ok) {
    throw new Error('Failed to update temperature')
  }
  return response.json()
}

export async function deleteTemperature(id: string): Promise<void> {
  const response = await fetch(`${API_BASE}/api/temperatures/${id}`, {
    method: 'DELETE',
  })
  if (!response.ok) {
    throw new Error('Failed to delete temperature')
  }
}
