
    <template>
        <nav class="navbar navbar-light bg-white shadow-sm">
            <div class="container-fluid">
                <div class="d-flex align-items-center">
                    <img src="/light-logo.svg" alt="Sutter Health Logo" class="me-3" style="width: 200px; height: 40px;" />
                    <span class="navbar-text text-teal">Sutter Analytics Marketplace</span>
                </div>
                <div class="d-flex align-items-center">
                    <a href="#" class="text-dark me-3">My Library</a>
                    <a href="#" class="text-dark me-3">My Requests</a>
                    <img src="../assets/user.jpg"
                         :title="userTitle"
                         alt="User Profile"
                         class="rounded-circle"
                         style="width: 40px; height: 40px; cursor: help;" />
                </div>
            </div>
        </nav>
    </template>

    <script>
        import { ref, onMounted, computed } from 'vue';
        import { fetchCurrentUser } from '../services/api';

        export default {
            name: 'Navbar',
            setup() {
                const me = ref(null);
                onMounted(async () => {
                    try { me.value = await fetchCurrentUser(); } catch { me.value = null; }
                });
                const userTitle = computed(() => {
                    if (!me.value) return 'Not signed in';
                    const name = me.value.displayName || me.value.userPrincipalName || 'User';
                    const role = me.value.userType || 'User';
                    return `${name}\nRole: ${role}`;
                });
                return { userTitle };
            }
        };
    </script>

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
