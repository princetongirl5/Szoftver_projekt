// Mirrors the backend EquipmentDto (GET /api/equipment).
export interface Equipment {
  id: number;
  name: string;
  /** Home items can be owned at home; Gym items exist only in a gym. */
  location: 'Home' | 'Gym';
}
