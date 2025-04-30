<template>
    <div class="container my-5">
        <div class="d-flex justify-content-between align-items-center mb-4">
            <h2 class="text-dark">Browse by Domain</h2>
            <a href="#" class="text-teal">Explore All <i class="bi bi-arrow-right"></i></a>
        </div>
        <div class="row row-cols-1 row-cols-md-4 g-4">
            <div class="col" v-for="domain in domains" :key="domain.id">
                <router-link :to="{
            name: 'DomainItems',
            params: {
              domainName: domain.value
                .toLowerCase()
                .replace(/ & /g, '-')
                .replace(/ /g, '-'),
            },
          }"
                             class="text-decoration-none">
                    <DomainTile :title="domain.value" :description="domain.value" />
                </router-link>
            </div>
        </div>
    </div>
</template>

<script>
    export default {
        name: 'DomainGrid',
        props: {
            domains: {
                type: Array,
                required: true,
            },
        },
    };
</script>

<style scoped>
    h2 {
        font-size: 1.5rem;
        font-weight: 500;
    }
</style>
