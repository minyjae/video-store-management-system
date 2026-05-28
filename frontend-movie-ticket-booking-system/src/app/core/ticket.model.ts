export interface Ticket {
  id: string;
  movieName: string;
  seatCode: string;
  showtime: string;
  pricePaid: number;
  referenceCode: string;
  qrCodeBase64: string;
  issuedAt: string;
}
