<template>
    <div class="hero-section">
        <div class="container">
            <h1 class="text-white mb-4">
                Hello Yusup, what would you like to search for today?
            </h1>
            <div class="input-group w-50">
                <span class="input-group-text bg-white border-0">
                    <i class="bi bi-search"></i>
                </span>
                <input type="text"
                       class="form-control border-0 py-2"
                       placeholder="Search by keyword, domain, asset type, etc... (press Enter)"
                       :value="search"
                       @input="$emit('update:search', $event.target.value)"
                       @keyup.enter="$emit('search-submit', $event.target.value)" />
            </div>
        </div>
    </div>
</template>

<script>
    export default {
        name: 'HeroSection',
        props: {
            search: {
                type: String,
                default: '',
            },
        },
        emits: ['update:search', 'search-submit'],
    };
</script>

<style scoped>
    .hero-section {
        background-image: url('/hero-bg.jpg');
        background-size: cover;
        background-position: center;
        padding: 40px 0;
        min-height: 200px;
        display: flex;
        align-items: center;
    }
</style>
