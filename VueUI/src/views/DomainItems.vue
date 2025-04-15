<template>
  <div>
    <Navbar />
    <HeroSection :domain="domainNameFormatted" />
    <div class="content-area">
      <div class="row">
        <div class="col-md-3">
          <FilterSidebar />
        </div>
        <div class="col-md-9">
          <ItemGrid />
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import Navbar from '../components/Navbar.vue';
import HeroSection from '../components/HeroSection.vue';
import FilterSidebar from '../components/FilterSidebar.vue';
import ItemGrid from '../components/ItemGrid.vue';

export default {
  name: 'DomainItems',
  components: {
    Navbar,
    HeroSection,
    FilterSidebar,
    ItemGrid,
  },
  props: {
    domainName: {
      type: String,
      required: true,
    },
  },
  computed: {
    domainNameFormatted() {
      return this.domainName
        .replace(/-/g, ' ')
        .replace(/\b\w/g, char => char.toUpperCase());
    },
  },
};
</script>

<style scoped>
.content-area {
  height: calc(100vh - 270px); /* Navbar (70px) + HeroSection (200px) */
  overflow-y: auto;
  padding: 0 15px;
}

.row {
  height: 100%;
}
</style>