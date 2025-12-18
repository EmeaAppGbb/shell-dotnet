export interface WeatherForecast {
  date: string
  temperatureC: number
  temperatureF: number
  summary: string | null
}

export interface TemperatureMeasurement {
  id: string
  location: string
  temperatureC: number
  temperatureF: number
  recordedAt: string
}

export interface CreateTemperatureMeasurement {
  location: string
  temperatureC: number
  recordedAt?: string
}

export interface UpdateTemperatureMeasurement {
  location?: string
  temperatureC?: number
  recordedAt?: string
}
