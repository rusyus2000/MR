<template>
  <div>
    <Navbar />
    <HeroSection />
    <div class="content-area">
      <DomainGrid />
    </div>
  </div>
</template>

<script>
import Navbar from '../components/Navbar.vue';
import HeroSection from '../components/HeroSection.vue';
import DomainGrid from '../components/DomainGrid.vue';

export default {
  name: 'Dashboard',
  components: {
    Navbar,
    HeroSection,
    DomainGrid,
  },
};
</script>

<style scoped>
.content-area {
  padding: 0 15px;
}
</style>