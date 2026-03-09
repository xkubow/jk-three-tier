<script setup>
import { ref } from "vue";

const loading = ref(false);
const value = ref("");
const error = ref("");

const loadConfiguration = async () => {
  loading.value = true;
  value.value = "";
  error.value = "";

  try {
    const response = await fetch("/api/configuration/HelloWorld");

    if (!response.ok) {
      throw new Error(`Request failed with status ${response.status}`);
    }

    const data = await response.json();
    value.value = data.value;
  } catch (e) {
    error.value = e instanceof Error ? e.message : "Unknown error";
  } finally {
    loading.value = false;
  }
};
</script>

<template>
  <main class="page">
    <div class="card">
      <h1>JK Three Tier Demo</h1>
      <p>
        Click the button to call the backend API and load configuration value
        from DB.
      </p>

      <button
        class="action-button"
        @click="loadConfiguration"
        :disabled="loading"
      >
        {{ loading ? "Loading..." : "Load HelloWorld value" }}
      </button>

      <p v-if="value" class="success">
        Returned value: <strong>{{ value }}</strong>
      </p>

      <p v-if="error" class="error">Error: {{ error }}</p>
    </div>
  </main>
</template>

<style scoped>
.page {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #f4f6f8;
  font-family: Arial, sans-serif;
}

.card {
  width: 100%;
  max-width: 500px;
  padding: 32px;
  background: white;
  border-radius: 12px;
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.08);
  text-align: center;
}

h1 {
  margin-bottom: 12px;
}

p {
  margin-bottom: 16px;
}

.action-button {
  padding: 12px 20px;
  border: none;
  border-radius: 8px;
  background: #1976d2;
  color: white;
  font-size: 16px;
  cursor: pointer;
}

.action-button:disabled {
  background: #90caf9;
  cursor: not-allowed;
}

.success {
  margin-top: 20px;
  color: #1b5e20;
}

.error {
  margin-top: 20px;
  color: #c62828;
}
</style>
