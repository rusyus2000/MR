<template>
  <div class="item-grid">
    <div class="d-flex justify-content-between align-items-center mb-4">
      <span class="text-muted">Showing {{ paginatedItems.length }} of {{ items.length }} results</span>
      <div class="d-flex align-items-center">
        <label for="sortBy" class="me-2">Sort by:</label>
        <select id="sortBy" class="form-select form-select-sm" v-model="sortBy">
          <option>Most Relevant</option>
          <option>Alphabetical</option>
        </select>
        <button class="btn btn-sm btn-outline-secondary ms-2">
          <i class="bi bi-grid"></i>
        </button>
        <button class="btn btn-sm btn-outline-secondary ms-1">
          <i class="bi bi-list"></i>
        </button>
      </div>
    </div>

    <div class="row row-cols-1 row-cols-md-3 g-4">
      <div class="col" v-for="(item, index) in paginatedItems" :key="index">
        <DomainCard
          :title="item.title"
          :description="item.description"
          :asset-types="item.assetTypes"
          :show-asset-type="true"
          :show-request-access="true"
          :has-access="item.hasAccess"
        />
      </div>
    </div>

    <!-- Pagination -->
    <div class="d-flex justify-content-between align-items-center mt-4">
      <div class="d-flex align-items-center">
        <label for="itemsPerPage" class="me-2">Items per page</label>
        <select id="itemsPerPage" class="form-select form-select-sm" v-model="itemsPerPage" style="width: auto;">
          <option>15</option>
          <option>30</option>
          <option>50</option>
        </select>
      </div>
      <div class="text-muted">
        {{ (currentPage - 1) * itemsPerPage + 1 }}-{{ Math.min(currentPage * itemsPerPage, items.length) }} of {{ items.length }}
      </div>
      <div>
        <button class="btn btn-sm btn-outline-secondary me-1" :disabled="currentPage === 1" @click="currentPage--">
          <i class="bi bi-chevron-left"></i>
        </button>
        <button class="btn btn-sm btn-outline-secondary" :disabled="currentPage === totalPages" @click="currentPage++">
          <i class="bi bi-chevron-right"></i>
        </button>
      </div>
    </div>
  </div>
</template>

<script>
import DomainCard from './DomainCard.vue';

export default {
  name: 'ItemGrid',
  components: {
    DomainCard,
  },
  data() {
    return {
      sortBy: 'Most Relevant',
      itemsPerPage: 15,
      currentPage: 1,
      items: [
        { title: 'Enterprise Refer Dashboard Monthly', description: 'Comprehensive, self-service analytic application which provides insights into referrals, and to-from patterns within the Sutter network, strengthening community relationships. The application is expected to include known...', assetTypes: ['Dashboard', 'Featured'], hasAccess: false },
        { title: 'Enterprise Refer Dashboard Daily', description: 'Comprehensive, self-service analytic application which provides insights into referrals, and to-from patterns within the Sutter network, strengthening community relationships. The application is expected to surface known...', assetTypes: ['Dashboard'], hasAccess: true },
        { title: 'Analytic Navigator', description: 'Dashboards used by decision makers to optimize operational efficiency by improving resource allocation, reducing waste, and streamlining processes.', assetTypes: ['Application'], hasAccess: false },
        { title: 'Hospital Capacity', description: 'This dashboard is the sole source for capacity information across Sutter Health. The dashboard is used for assessing capacity within hospitals, and includes capacity for the overall hospital, including a breakdown of capacity for the hospital, including a breakdown of capacity by bed, the current occupancy, and...', assetTypes: ['Dashboard', 'Featured'], hasAccess: true },
        { title: 'FLASH Report', description: 'Targeted dashboard that includes the underlying FLASH Reports: Acute, Foundation, ASC, EPIC.', assetTypes: ['Dashboard'], hasAccess: false },
        { title: 'System Capacity', description: 'This dashboard is the sole source for capacity information across Sutter Health. The dashboard is used by capacity managers and staff. This dashboard displays overall capacity for the entire Sutter system, at a high level. It includes key metrics for each of the locations, broken down...', assetTypes: ['Dashboard'], hasAccess: false },
        { title: 'Visits', description: 'SutterView data models provide self-service data exploration tools. This data model contains a subset of ambulatory visits and related information. It does not include information about...', assetTypes: ['Data Model'], hasAccess: true },
        { title: 'FTE Flash Report - Weekly', description: 'Targeted dashboards with metrics specific to FTE. Metrics cover all markets, entities, departments, divisions and specialties.', assetTypes: ['Report'], hasAccess: false },
        { title: 'ASC Flash Report - Monthly', description: 'Targeted dashboards with metrics specific to ASC. Metrics cover all markets, entities, divisions and specialties.', assetTypes: ['Report'], hasAccess: true },
        { title: 'Detail Summary - MOR (Monthly Operating Review)', description: 'Monthly operating review of the top metrics across all markets and entities with additional dimensions focusing on Need, Attention, Finance, Clinical Operations, Access to Care, People & Workforce, Growth, Quality and Physician Recruitment.', assetTypes: ['Report', 'Featured'], hasAccess: false },
        { title: 'Foundation Flash Report - Weekly', description: 'Targeted dashboards with metrics specific to Foundation. Metrics cover all markets, entities, divisions and specialties.', assetTypes: ['Report'], hasAccess: false },
        { title: 'Foundation Flash Report - Monthly', description: 'Targeted dashboards with metrics specific to Foundation. Metrics cover all markets, entities, divisions and specialties.', assetTypes: ['Report'], hasAccess: true },
        { title: 'ASC Flash Report - Weekly', description: 'Targeted dashboards with metrics specific to ASC. Metrics cover all markets, entities, divisions and specialties.', assetTypes: ['Report'], hasAccess: false },
        { title: 'Acute Flash Report - Weekly', description: 'Targeted dashboards with metrics specific to Acute. Metrics cover all markets, entities, divisions and specialties.', assetTypes: ['Report'], hasAccess: true },
        { title: 'Acute Flash Report - Monthly', description: 'Targeted dashboards with metrics specific to Acute. Metrics cover all markets, entities, divisions and specialties.', assetTypes: ['Report'], hasAccess: false },
        { title: 'Sample Dashboard 1', description: 'A sample dashboard for testing purposes.', assetTypes: ['Dashboard'], hasAccess: true },
        { title: 'Sample Dashboard 2', description: 'A sample dashboard for testing purposes.', assetTypes: ['Dashboard'], hasAccess: false },
        { title: 'Sample Report 1', description: 'A sample report for testing purposes.', assetTypes: ['Report'], hasAccess: true },
        { title: 'Sample Report 2', description: 'A sample report for testing purposes.', assetTypes: ['Report'], hasAccess: false },
        { title: 'Sample Report 3', description: 'A sample report for testing purposes.', assetTypes: ['Report'], hasAccess: true },
      ],
    };
  },
  computed: {
    totalPages() {
      return Math.ceil(this.items.length / this.itemsPerPage);
    },
    paginatedItems() {
      const start = (this.currentPage - 1) * this.itemsPerPage;
      const end = start + this.itemsPerPage;
      let sortedItems = this.items;
      if (this.sortBy === 'Alphabetical') {
        sortedItems = [...this.items].sort((a, b) => a.title.localeCompare(b.title));
      }
      return sortedItems.slice(start, end);
    },
  },
};
</script>

<style scoped>
.item-grid {
  padding: 20px;
}

.form-select-sm {
  width: auto;
}

.btn-outline-secondary {
  border-color: #e0e0e0;
}

.text-muted {
  font-size: 0.9rem;
}
</style>