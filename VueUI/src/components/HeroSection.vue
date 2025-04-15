<template>
  <div class="hero-section">
    <div class="container">
      <h1 class="text-white mb-4">Hello Yusup, what would you like to search for today?</h1>
      <div class="input-group w-50">
        <span class="input-group-text bg-white border-0">
          <i class="bi bi-search"></i>
        </span>
        <input type="text" class="form-control border-0 py-2" placeholder="Search by keyword, domain, asset type, etc..." />
      </div>
      <div v-if="domain" class="mt-3">
        <span class="badge bg-teal text-white">Domain: {{ domain }}</span>
      </div>
    </div>
  </div>
</template>

<script>
export default {
  name: 'HeroSection',
  props: {
    domain: {
      type: String,
      default: '',
    },
  },
};
</script>

<style scoped>
.hero-section {
  background-image: url('/hero-bg.jpg');
  background-size: cover;
  background-position: center;
  padding: 40px 0; /* Reduced padding to make the section more compact */
  min-height: 200px; /* Set a minimum height to prevent it from growing too large */
  display: flex;
  align-items: center; /* Center content vertically */
}

.badge.bg-teal {
  background-color: #00A89E;
}
</style>