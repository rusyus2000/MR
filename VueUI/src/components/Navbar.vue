
    <template>
        <nav class="navbar navbar-light bg-white shadow-sm">
            <div class="container-fluid">
                <div class="d-flex align-items-center">
                    <img src="/light-logo.svg" alt="Sutter Health Logo" class="me-3" style="width: 200px; height: 40px;" />
                    <span class="navbar-text text-teal">Sutter Analytics Marketplace</span>
                </div>
                <div class="d-flex align-items-center user-display" :title="userTitle">
                    <span class="me-2">{{ displayName }}</span>
                    <i class="bi bi-person-circle fs-4"></i>
                </div>
            </div>
        </nav>
    </template>

    <script>
        import { ref, onMounted, computed } from 'vue';
        import { getCurrentUserCached, fetchIntranetUser, updateCurrentUserProfile } from '../services/api';

        export default {
            name: 'Navbar',
            setup() {
                const me = ref(null);
                onMounted(async () => {
                    try {
                        me.value = await getCurrentUserCached();
                        // If missing display name or email, try intranet API and push to server
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
                const userTitle = computed(() => {
                    if (!me.value) return 'Not signed in';
                    const name = me.value.displayName || me.value.userPrincipalName || 'User';
                    const role = me.value.userType || 'User';
                    return `${name}\nRole: ${role}`;
                });
                const displayName = computed(() => me.value?.displayName || me.value?.userPrincipalName || '');
                return { userTitle, displayName };
            }
        };
    </script>

    <style scoped>
    .user-display { cursor: default; }
    </style>

    <style scoped>
        .navbar-brand,
        .navbar-text {
            padding-top: 0 !important;
            padding-bottom: .5rem;
            font-weight: 700;
            font-size: 1.5rem
        }

        .navbar {
            padding: 10px 20px;
            height: 70px;
            display: flex;
            align-items: center;
        }
    </style>
