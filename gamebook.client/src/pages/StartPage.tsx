import { useState, useEffect } from 'react';
import {Alert} from '../components/common';
import requireAuth from '../hocs/requireAuth';


const StartPage = () => {
    return (
        <div>
            <h1>Start</h1>     
        </div>
    )
}

export default requireAuth(StartPage);