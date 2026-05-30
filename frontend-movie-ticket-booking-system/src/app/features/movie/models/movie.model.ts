export enum MovieCategory {
  Action = 'Action',
  Comedy = 'Comedy',
  Drama = 'Drama',
  Horror = 'Horror',
  Romance = 'Romance',
  Science_Fiction = 'Science_Fiction',
  Thriller = 'Thriller',
  Fantasy = 'Fantasy',
}

export interface Movie {
  id: string;
  title: string;
  plot: string;
  price: number;
  duration: string;
  category: MovieCategory;
  posterUrl?: string;
  createdAt: string;
  updatedAt: string;
}
