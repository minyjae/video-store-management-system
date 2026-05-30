export enum SeatStatus {
  Available = 'Available',
  Locked = 'Locked',
  Booked = 'Booked',
}

export enum SeatType {
  Normal = 'Normal',
  VIP = 'VIP',
}

export interface Seat {
  id: string;
  showtimeId: string;
  seatCode: string;
  type: SeatType;
  price: number;
  status: SeatStatus;
}
