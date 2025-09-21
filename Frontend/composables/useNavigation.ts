  import type { NavigationItem } from '~/types'

  export const navigationItems: NavigationItem[] = [
    {
      title: 'Dashboard',
      icon: 'mdi-view-dashboard',
      children: [
        {
           title: 'Dashboard',
      icon: 'mdi-view-dashboard',
      to: '/dashboard',
        }
        
      ]
    },
    
    {
      title: 'User Management',
      icon: 'mdi-account-group',
      children: [
        {
          title: 'Users',
          icon: 'mdi-account-multiple',
          to: '/users',
          permission: 'Users.Read'
        },
        {
          title: 'Roles',
          icon: 'mdi-shield-account',
          to: '/roles',
          permission: 'Roles.Read'
        },
        {
          title: 'Permissions',
          icon: 'mdi-key',
          to: '/permissions',
          permission: 'Permissions.Read'
        }
      ]
    },
    
    {
      title: 'Settings',
      icon: 'mdi-cog',
      to: '/settings',
      roles: ['Admin', 'SuperAdmin']
    }
  ]

  export const filterNavigationByPermissions = (
    items: NavigationItem[],
    hasPermission: (permission: string) => boolean,
    hasRole: (role: string) => boolean
  ): NavigationItem[] => {
    return items.filter(item => {
      // Check permission requirement
      if (item.permission && !hasPermission(item.permission)) {
        return false
      }

      // Check role requirement
      if (item.roles && !item.roles.some(role => hasRole(role))) {
        return false
      }

      // If item has children, filter them recursively
      if (item.children) {
        item.children = filterNavigationByPermissions(item.children, hasPermission, hasRole)
        // Keep parent if it has at least one visible child
        return item.children.length > 0
      }

      return true
    })
  } 