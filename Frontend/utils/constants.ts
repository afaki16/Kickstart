export const USER_STATUS_OPTIONS = [
  { title: 'Active', value: 1, color: 'success' },
  { title: 'Inactive', value: 2, color: 'warning' },
  { title: 'Banned', value: 3, color: 'error' },
  { title: 'Pending Verification', value: 4, color: 'info' }
]

export const PERMISSION_TYPE_OPTIONS = [
  { title: 'Create', value: 1 },
  { title: 'Read', value: 2 },
  { title: 'Update', value: 3 },
  { title: 'Delete', value: 4 },
  { title: 'Manage', value: 5 }
]

export const ITEMS_PER_PAGE_OPTIONS = [
  { title: '10', value: 10 },
  { title: '25', value: 25 },
  { title: '50', value: 50 },
  { title: '100', value: 100 }
]

export const TABLE_HEADERS = {
  users: [
    { title: 'Name', key: 'fullName', sortable: true },
    { title: 'Email', key: 'email', sortable: true },
    { title: 'Phone', key: 'phoneNumber', sortable: false },
    { title: 'Status', key: 'status', sortable: true },
    { title: 'Roles', key: 'roles', sortable: false },
    { title: 'Created', key: 'createdDate', sortable: true },
    { title: 'Actions', key: 'actions', sortable: false, align: 'center' }
  ],
  
  roles: [
    { title: 'Name', key: 'name', sortable: true },
    { title: 'Description', key: 'description', sortable: false },
    { title: 'Type', key: 'isSystemRole', sortable: true },
    { title: 'Permissions', key: 'permissions', sortable: false },
    { title: 'Created', key: 'createdDate', sortable: true },
    { title: 'Actions', key: 'actions', sortable: false, align: 'center' }
  ],
  
  permissions: [
    { title: 'Name', key: 'name', sortable: true },
    { title: 'Resource', key: 'resource', sortable: true },
    { title: 'Type', key: 'type', sortable: true },
    { title: 'Description', key: 'description', sortable: false },
    { title: 'Created', key: 'createdDate', sortable: true }
  ]
} 