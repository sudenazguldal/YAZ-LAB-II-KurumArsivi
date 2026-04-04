import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
    thresholds: {
        http_req_failed: ['rate<0.05'],
        http_req_duration: ['p(95)<1500'],
    },
};

const BASE_URL = __ENV.BASE_URL || 'http://host.docker.internal:5000';
const USERNAME = __ENV.USERNAME || 'admin';
const PASSWORD = __ENV.PASSWORD || '123456';

export function setup() {
    const payload = JSON.stringify({
        username: USERNAME,
        password: PASSWORD,
    });

    const params = {
        headers: {
            'Content-Type': 'application/json',
        },
    };

    const res = http.post(`${BASE_URL}/api/auth/login`, payload, params);

    const ok = check(res, {
        'login status is 200': (r) => r.status === 200,
        'token exists': (r) => {
            try {
                return !!r.json('token');
            } catch (e) {
                return false;
            }
        },
    });

    if (!ok) {
        throw new Error('Login başarısız. Kullanıcı adı/şifreyi kontrol et.');
    }

    return {
        token: res.json('token'),
    };
}

export default function (data) {
    const authHeaders = {
        headers: {
            Authorization: `Bearer ${data.token}`,
            'Content-Type': 'application/json',
        },
    };

    if (__ITER % 2 === 0) {
        const res = http.get(`${BASE_URL}/api/auth/users`, authHeaders);

        check(res, {
            'GET /api/auth/users => 200': (r) => r.status === 200,
        });
    } else {
        const res = http.get(`${BASE_URL}/api/documents`, authHeaders);

        check(res, {
            'GET /api/documents => 200': (r) => r.status === 200,
        });
    }

    sleep(1);
}