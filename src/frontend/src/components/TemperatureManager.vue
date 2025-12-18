<script setup lang="ts">
import { ref, onMounted } from 'vue'
import type {
  TemperatureMeasurement,
  CreateTemperatureMeasurement,
  UpdateTemperatureMeasurement,
} from '@/types/weather'
import {
  getTemperatures,
  createTemperature,
  updateTemperature,
  deleteTemperature,
} from '@/services/api'

const measurements = ref<TemperatureMeasurement[]>([])
const loading = ref(true)
const error = ref<string | null>(null)
const showForm = ref(false)
const editingId = ref<string | null>(null)

const formData = ref<CreateTemperatureMeasurement>({
  location: '',
  temperatureC: 0,
})

async function loadMeasurements() {
  try {
    loading.value = true
    error.value = null
    measurements.value = await getTemperatures()
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Failed to load measurements'
  } finally {
    loading.value = false
  }
}

onMounted(loadMeasurements)

function resetForm() {
  formData.value = { location: '', temperatureC: 0 }
  editingId.value = null
  showForm.value = false
}

function openCreateForm() {
  resetForm()
  showForm.value = true
}

function openEditForm(measurement: TemperatureMeasurement) {
  formData.value = {
    location: measurement.location,
    temperatureC: measurement.temperatureC,
  }
  editingId.value = measurement.id
  showForm.value = true
}

async function handleSubmit() {
  try {
    if (editingId.value) {
      const updateData: UpdateTemperatureMeasurement = {
        location: formData.value.location,
        temperatureC: formData.value.temperatureC,
      }
      await updateTemperature(editingId.value, updateData)
    } else {
      await createTemperature(formData.value)
    }
    resetForm()
    await loadMeasurements()
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Failed to save measurement'
  }
}

async function handleDelete(id: string) {
  if (!confirm('Are you sure you want to delete this measurement?')) return
  try {
    await deleteTemperature(id)
    await loadMeasurements()
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Failed to delete measurement'
  }
}

function formatDateTime(dateStr: string): string {
  const date = new Date(dateStr)
  return date.toLocaleString('en-US', {
    month: 'short',
    day: 'numeric',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })
}
</script>

<template>
  <div class="temperature-manager">
    <div class="header">
      <h2>🌡️ Temperature Measurements</h2>
      <button class="btn btn-primary" @click="openCreateForm">+ Add Measurement</button>
    </div>

    <div v-if="error" class="error">{{ error }}</div>

    <!-- Form Modal -->
    <div v-if="showForm" class="modal-overlay" @click.self="resetForm">
      <div class="modal">
        <h3>{{ editingId ? 'Edit' : 'Add' }} Measurement</h3>
        <form @submit.prevent="handleSubmit">
          <div class="form-group">
            <label for="location">Location</label>
            <input
              id="location"
              v-model="formData.location"
              type="text"
              required
              placeholder="e.g., New York"
            />
          </div>
          <div class="form-group">
            <label for="temperature">Temperature (°C)</label>
            <input
              id="temperature"
              v-model.number="formData.temperatureC"
              type="number"
              step="0.1"
              required
            />
          </div>
          <div class="form-actions">
            <button type="button" class="btn btn-secondary" @click="resetForm">Cancel</button>
            <button type="submit" class="btn btn-primary">
              {{ editingId ? 'Update' : 'Create' }}
            </button>
          </div>
        </form>
      </div>
    </div>

    <!-- Table -->
    <div v-if="loading" class="loading">Loading measurements...</div>

    <div v-else-if="measurements.length === 0" class="empty">
      No measurements recorded yet. Add one to get started!
    </div>

    <table v-else class="measurements-table">
      <thead>
        <tr>
          <th>Location</th>
          <th>Temperature</th>
          <th>Recorded At</th>
          <th>Actions</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="measurement in measurements" :key="measurement.id">
          <td>{{ measurement.location }}</td>
          <td>
            <span class="temp">{{ measurement.temperatureC.toFixed(1) }}°C</span>
            <span class="temp-secondary">({{ measurement.temperatureF.toFixed(1) }}°F)</span>
          </td>
          <td>{{ formatDateTime(measurement.recordedAt) }}</td>
          <td class="actions">
            <button class="btn btn-small" @click="openEditForm(measurement)">Edit</button>
            <button class="btn btn-small btn-danger" @click="handleDelete(measurement.id)">
              Delete
            </button>
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<style scoped>
.temperature-manager {
  margin-top: 1.25rem;
}

.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
  flex-wrap: wrap;
  gap: 1rem;
}

h2 {
  color: var(--color-heading);
  margin: 0;
}

.loading,
.empty {
  text-align: center;
  padding: 2rem;
  color: var(--color-text);
}

.error {
  background: #fee;
  color: #c00;
  padding: 1rem;
  border-radius: 8px;
  margin-bottom: 1rem;
}

/* Buttons */
.btn {
  padding: 0.5rem 1rem;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-size: 0.875rem;
  transition: opacity 0.2s;
}

.btn:hover {
  opacity: 0.9;
}

.btn-primary {
  background: linear-gradient(135deg, var(--color-accent), var(--color-accent-strong));
  color: white;
  box-shadow: 0 10px 24px rgba(37, 99, 235, 0.25);
}

.btn-secondary {
  background: var(--color-surface-muted);
  color: var(--color-heading);
  border: 1px solid var(--color-border);
}

.btn-danger {
  background: linear-gradient(135deg, #ef4444, #dc2626);
  color: white;
}

.btn-small {
  padding: 0.25rem 0.5rem;
  font-size: 0.75rem;
}

/* Modal */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.modal {
  background: var(--color-background);
  padding: 2rem;
  border-radius: 12px;
  min-width: 300px;
  max-width: 90%;
}

.modal h3 {
  margin-top: 0;
  margin-bottom: 1.5rem;
  color: var(--color-heading);
}

.form-group {
  margin-bottom: 1rem;
}

.form-group label {
  display: block;
  margin-bottom: 0.5rem;
  font-weight: 500;
  color: var(--color-text);
}

.form-group input {
  width: 100%;
  padding: 0.5rem;
  border: 1px solid var(--color-border);
  border-radius: 4px;
  font-size: 1rem;
  background: var(--color-surface-muted);
  color: var(--color-text);
}

.form-actions {
  display: flex;
  gap: 1rem;
  justify-content: flex-end;
  margin-top: 1.5rem;
}

/* Table */
.measurements-table {
  width: 100%;
  border-collapse: collapse;
  background: var(--color-surface);
  border-radius: 8px;
  overflow: hidden;
}

.measurements-table th,
.measurements-table td {
  padding: 1rem;
  text-align: left;
  border-bottom: 1px solid var(--color-border);
}

.measurements-table th {
  background: var(--color-surface-muted);
  font-weight: 600;
  color: var(--color-heading);
}

.measurements-table tr:last-child td {
  border-bottom: none;
}

.temp {
  font-weight: 600;
}

.temp-secondary {
  color: var(--color-text-muted, #888);
  font-size: 0.875rem;
  margin-left: 0.5rem;
}

.actions {
  display: flex;
  gap: 0.5rem;
}
</style>
