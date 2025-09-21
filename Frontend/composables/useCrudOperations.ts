// composables/useCrudOperations.ts
export const useCrudOperations = (config) => {
  const {
    loadItems,
    createItem,
    updateItem,
    deleteItem,
    itemName = 'öğe',
    toastMessages = {
      createSuccess: `${itemName} başarıyla oluşturuldu`,
      updateSuccess: `${itemName} başarıyla güncellendi`,
      deleteSuccess: `${itemName} başarıyla silindi`,
      loadError: `${itemName} yüklenirken hata oluştu`,
      createError: `${itemName} oluşturulurken hata oluştu`,
      updateError: `${itemName} güncellenirken hata oluştu`,
      deleteError: `${itemName} silinirken hata oluştu`
    }
  } = config

  const dialogManager = useDialogManager()
  const items = ref([])
  const isLoading = ref(false)
  const isDeleting = ref(false)

  // Load items
  const loadItemsData = async () => {
    try {
      isLoading.value = true
      const response = await loadItems()
      
      if (Array.isArray(response)) {
        items.value = response
      } else if (response && response.data) {
        items.value = response.data.items || response.data
      } else {
        items.value = []
      }
    } catch (error) {
      console.error(`Error loading ${itemName}:`, error)
      // TODO: Toast message ekle
      // await $toast.error(toastMessages.loadError)
      items.value = []
    } finally {
      isLoading.value = false
    }
  }

  // Handle submit (create/update)
  const handleSubmit = async (itemData) => {
    try {
      isLoading.value = true
      
      if (dialogManager.isEditMode.value && dialogManager.selectedItem.value) {
        await updateItem(dialogManager.selectedItem.value.id, itemData)
        // TODO: Toast message ekle
        // await $toast.success(toastMessages.updateSuccess)
      } else {
        await createItem(itemData)
        // TODO: Toast message ekle
        // await $toast.success(toastMessages.createSuccess)
      }
      
      dialogManager.closeCreateDialog()
      await loadItemsData()
    } catch (error) {
      const errorMessage = dialogManager.isEditMode.value 
        ? toastMessages.updateError 
        : toastMessages.createError
      console.error(`Error submitting ${itemName}:`, error)
      // TODO: Toast message ekle
      // await $toast.error(errorMessage)
    } finally {
      isLoading.value = false
    }
  }

  // Handle delete
  const confirmDelete = async () => {
    if (!dialogManager.itemToDelete.value) return
    
    try {
      isDeleting.value = true
      await deleteItem(dialogManager.itemToDelete.value.id)
      await loadItemsData()
      dialogManager.closeDeleteDialog()
      // TODO: Toast message ekle
      // await $toast.success(toastMessages.deleteSuccess)
    } catch (error) {
      console.error(`Error deleting ${itemName}:`, error)
      // TODO: Toast message ekle
      // await $toast.error(toastMessages.deleteError)
    } finally {
      isDeleting.value = false
    }
  }

  // Search handler
  const handleSearch = () => {
    loadItemsData()
  }

  return {
    // Data
    items,
    isLoading,
    isDeleting,
    
    // Dialog manager
    ...dialogManager,
    
    // Operations
    loadItemsData,
    handleSubmit,
    confirmDelete,
    handleSearch
  }
}