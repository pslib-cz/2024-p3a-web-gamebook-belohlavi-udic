interface Room {
    id: number;
    name: string;
    description: string;
}

interface Connection {
    id: number;
    roomId1: number;
    roomId2: number;
    connectionType: string;
}

interface Challenge {
    id: number;
    type: string;
    description: string;
    successOutcome: string;
    failureOutcome: string;
}

interface GameState {
    id: number;
    playerId: number;
    timestamp: Date;
    data: string;
}

interface Player {
    id: number;
    currentRoomId: number;
    hp: number;
    status: string;
}

export class GameService {
    private static BASE_URL = '/api';

    private static async fetchWithAuth(endpoint: string, options: RequestInit = {}, token: string) {
        if (!token) {
            throw new Error('No authentication token provided');
        }

        const headers = {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`,
            ...options.headers
        };

        const response = await fetch(`${this.BASE_URL}${endpoint}`, {
            ...options,
            headers
        });

        if (response.status === 401) {
            localStorage.removeItem('access_token');
            window.location.href = '/sign-in';
            throw new Error('Unauthorized');
        }

        if (!response.ok) {
            const errorData = await response.json();
            console.error("GameService: API Error:", errorData);
            throw new Error(`API call failed: ${response.statusText}, ${errorData.message}`);
        }

        const responseData = await response.json();
        console.log("GameService: API Response:", responseData);
        return responseData;
    }

    static async getCurrentRoom(token: string): Promise<Room> {
        return this.fetchWithAuth(`/rooms/current`, {}, token);
    }

    static async getRoomById(roomId: number, token: string): Promise<Room> {
        return this.fetchWithAuth(`/rooms/${roomId}`, {}, token);
    }

    static async getConnections(roomId: number, token: string): Promise<Connection[]> {
        return this.fetchWithAuth(`/connections/${roomId}`, {}, token);
    }

    static async movePlayer(connectionId: number, token: string): Promise<{
        newRoomId: number;
        playerHp: number;
        playerStatus: string;
    }> {
        return this.fetchWithAuth(`/players/current/move`, {
            method: 'PUT',
            body: JSON.stringify({ connectionId })
        }, token);
    }

    static async getChallenge(challengeId: number, token: string): Promise<Challenge> {
        return this.fetchWithAuth(`/challenges/${challengeId}`, {}, token);
    }

    static async attemptChallenge(challengeId: number, token: string): Promise<{
        success: boolean;
        outcome: string;
        playerHp: number;
    }> {
        return this.fetchWithAuth(`/challenges/${challengeId}/attempt`, {
            method: 'POST'
        }, token);
    }

    static async getCurrentPlayer(token: string): Promise<Player> {
        return this.fetchWithAuth(`/players/current`, {}, token);
    }

    static async updatePlayerStatus(status: string, token: string): Promise<Player> {
        return this.fetchWithAuth(`/players/current/status`, {
            method: 'PUT',
            body: JSON.stringify({ status })
        }, token);
    }

    static async updatePlayerHp(hp: number, token: string): Promise<Player> {
        return this.fetchWithAuth(`/players/current/hp`, {
            method: 'PUT',
            body: JSON.stringify({ hp })
        }, token);
    }

    static async saveGameState(state: Partial<GameState>, token: string): Promise<GameState> {
        return this.fetchWithAuth('/game-states/save', {
            method: 'POST',
            body: JSON.stringify(state)
        }, token);
    }

    static async loadGameState(gameStateId: number, token: string): Promise<GameState> {
        return this.fetchWithAuth(`/game-states/${gameStateId}`, {}, token);
    }

    static async getLatestGameState(token: string): Promise<GameState | null> {
        return this.fetchWithAuth('/game-states/latest', {}, token);
    }

    static async startNewGame(token: string): Promise<{
        playerId: number;
        initialRoomId: number;
        gameStateId: number;
        playerHp: number;
        playerStatus: string;
    }> {
        return this.fetchWithAuth('/players/current', { method: 'GET' }, token);
    }

    static async endGame(token: string): Promise<void> {
        return this.fetchWithAuth('/game/end', {
            method: 'POST'
        }, token);
    }

    static isTokenExpired(token: string): boolean {
        if (!token) return true;
        try {
            const [, payload] = token.split('.');
            const decodedPayload = JSON.parse(atob(payload));
            return decodedPayload.exp * 1000 < Date.now();
        } catch {
            return true;
        }
    }
}