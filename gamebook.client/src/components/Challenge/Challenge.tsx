import { useEffect } from 'react';
import { useChallenge } from '../../contexts/ChallengeContext';
import { useGame } from '../../contexts/GameContext';

interface ChallengeProps {
    challengeId: number;
    onComplete?: (success: boolean) => void;
}

export default function Challenge({ challengeId, onComplete }: ChallengeProps) {
    const { state: challengeState, startChallenge, attemptChallenge } = useChallenge();
    const { state: gameState } = useGame();

    useEffect(() => {
        startChallenge(challengeId);
    }, [challengeId, startChallenge]);

    useEffect(() => {
        if (challengeState.outcome !== null && onComplete) {
            onComplete(challengeState.outcome === 'success');
        }
    }, [challengeState.outcome, onComplete]);

    if (challengeState.loading) {
        return (
            <div className="flex justify-center items-center h-64">
                <div className="loading-spinner" />
            </div>
        );
    }

    if (challengeState.error) {
        return (
            <div className="alert alert-error">
                {challengeState.error}
            </div>
        );
    }

    if (!challengeState.currentChallenge) {
        return null;
    }

    const { currentChallenge, outcome } = challengeState;

    return (
        <div className="game-container p-6 max-w-2xl mx-auto">
            <div className="mb-6">
                <h2 className="text-2xl font-bold text-green-400 mb-4">
                    {currentChallenge.type}
                </h2>
                <p className="text-gray-300 mb-6">
                    {currentChallenge.description}
                </p>

                {outcome === null ? (
                    <div className="flex flex-col gap-4">
                        <div className="bg-gray-800 p-4 rounded-lg">
                            <div className="mb-2 text-sm text-gray-400">
                                Tvoje zdraví: {gameState.hp}
                            </div>
                            <div className="health-bar">
                                <div
                                    className="health-bar__fill"
                                    style={{ width: `${gameState.hp}%` }}
                                />
                            </div>
                        </div>

                        <button
                            onClick={attemptChallenge}
                            disabled={challengeState.loading || gameState.hp <= 0}
                            className="btn btn-primary w-full"
                        >
                            Pokusit se o p?ekonání
                        </button>
                    </div>
                ) : (
                    <div className={`p-4 rounded-lg ${outcome === 'success' ? 'bg-green-900/50' : 'bg-red-900/50'
                        }`}>
                        <p className="text-lg font-medium mb-2">
                            {outcome === 'success' ? 'Úsp?ch!' : 'Neúsp?ch!'}
                        </p>
                        <p className="text-gray-300">
                            {outcome === 'success'
                                ? currentChallenge.successOutcome
                                : currentChallenge.failureOutcome}
                        </p>
                    </div>
                )}
            </div>
        </div>
    );
}