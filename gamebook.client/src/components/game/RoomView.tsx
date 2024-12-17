import './RoomView.css';

interface RoomViewProps {
    room: {
        id: number;
        name: string;
        description: string;
    };
    connections: {
        id: number;
        connectionType: string;
        roomId1: number;
        roomId2: number;
    }[];
    onMove: (connectionId: number) => void;
}

export const RoomView = ({ room, connections, onMove }: RoomViewProps) => {
    return (
        <div className="room-view">
            <div className="room-content">
                <h2 className="room-title">{room.name}</h2>
                <p className="room-description">{room.description}</p>

                <div className="room-connections">
                    {connections.map(connection => (
                        <button
                            key={connection.id}
                            className={`connection-button ${connection.connectionType.toLowerCase()}`}
                            onClick={() => onMove(connection.id)}
                        >
                            {connection.connectionType === 'NORTH' && '⬆️'}
                            {connection.connectionType === 'SOUTH' && '⬇️'}
                            {connection.connectionType === 'EAST' && '➡️'}
                            {connection.connectionType === 'WEST' && '⬅️'}
                            {` Jít ${connection.connectionType}`}
                        </button>
                    ))}
                </div>
            </div>
        </div>
    );
};