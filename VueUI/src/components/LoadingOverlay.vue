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
  position: absolute;
  inset: 0;
  background: rgba(255, 255, 255, 0.7);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 10;
}
.loading-content { text-align: center; }
</style>

