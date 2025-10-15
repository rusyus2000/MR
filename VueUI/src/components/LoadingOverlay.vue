<template>
  <div class="loading-overlay">
    <div class="loading-content">
      <div class="spinner-border text-primary" role="status" aria-label="Loading"></div>
      <div v-if="message" class="mt-2 text-muted small">{{ message }}</div>
    </div>
  </div>
</template>

<script>
export default {
  name: 'LoadingOverlay',
  props: {
    message: { type: String, default: '' }
  }
};
</script>

<style scoped>
.loading-overlay {
  position: fixed; /* pin to viewport so it stays visible while scrolling */
  inset: 0;
  background: rgba(255, 255, 255, 0.7);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 2000; /* above page content, below modals if any */
}
.loading-content { text-align: center; }
</style>
