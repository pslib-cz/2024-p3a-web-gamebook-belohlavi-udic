export interface Exit {
    id: number;
    direction: string;
    targetRoomId: number;
}

export interface Room {
    id: number;
    name: string;
    description: string;
    imagePath: string;
    imageUrl?: string;
    exits: Exit[];
}

export interface Player {
    id: number;
    hp: number;
    currentRoomId: number;
}