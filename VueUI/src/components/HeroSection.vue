<template>
    <div class="hero-section">
        <div class="container">
            <h1 class="text-white mb-4">
                Hello {{ firstName }}, what would you like to search for today?
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
    import { ref, computed, onMounted } from 'vue';
    import { getCurrentUserCached, fetchIntranetUser, updateCurrentUserProfile } from '../services/api';

    export default {
        name: 'HeroSection',
        props: {
            search: {
                type: String,
                default: '',
            },
        },
        emits: ['update:search', 'search-submit'],
        setup() {
            const me = ref(null);
            const firstName = computed(() => {
                const dn = me.value?.displayName || me.value?.userPrincipalName || '';
                if (!dn) return 'there';
                // Expected format: "LastName, FirstName ..." → take part after comma, then first token
                const parts = dn.split(',');
                let given = parts.length > 1 ? parts[1].trim() : dn.trim();
                // Take first token of given name (handles middle names)
                given = given.split(/\s+/)[0] || given;
                return given;
            });
            onMounted(async () => {
                try {
                    me.value = await getCurrentUserCached();
                    if (!me.value || !me.value.displayName || !me.value.email) {
                        const intranet = await fetchIntranetUser();
                        const u = intranet?.user;
                        if (u && (u.name || u.email || u.networkId)) {
                            await updateCurrentUserProfile({ displayName: u.name, email: u.email, networkId: u.networkId }).catch(() => {});
                            me.value = await getCurrentUserCached(true);
                        }
                    }
                } catch { me.value = null; }
            });
            return { firstName };
        }
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
