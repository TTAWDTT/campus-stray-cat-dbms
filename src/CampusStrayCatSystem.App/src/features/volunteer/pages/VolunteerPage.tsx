import { Card, Icon } from 'animal-island-ui'
import { useNavigate } from 'react-router-dom'
import type { KeyboardEvent } from 'react'
import { useEffect, useState } from 'react'
import { VolunteerService } from '../../../services/volunteer.service'

export function VolunteerPage() {
    const navigate = useNavigate()
    const [pendingCount, setPendingCount] = useState(0)

    useEffect(() => {
        VolunteerService.getPendingApplications()
            .then((data) => setPendingCount(data.length))
            .catch(() => setPendingCount(0))
    }, [])

    const activateCard = (action: () => void) => (e: KeyboardEvent<HTMLDivElement>) => {
        if (e.key === 'Enter' || e.key === ' ') {
            e.preventDefault()
            action()
        }
    }

    return (
        <div className="finance-hero-grid">
            <div className="finance-hero-card-hit" role="button" tabIndex={0}
                aria-label="领养审核"
                onClick={() => navigate('/volunteer/adoptions')}
                onKeyDown={activateCard(() => navigate('/volunteer/adoptions'))}>
                <Card color="app-teal" className="finance-hero-card">
                    <div className="finance-hero-card-inner">
                        <span className="finance-hero-icon">
                            <Icon name="icon-chat" size={28} />
                        </span>
                        <h2>领养审核</h2>
                        <p>处理待审核的领养申请，仔细评估每一位申请人是否适合领养。</p>
                        {pendingCount > 0 && (
                            <span className="volunteer-pending-badge">
                                <strong>{pendingCount}</strong>
                                <small>条待审核</small>
                            </span>
                        )}
                    </div>
                </Card>
            </div>

            <div className="finance-hero-card-hit" role="button" tabIndex={0}
                aria-label="回访汇总"
                onClick={() => navigate('/volunteer/visits')}
                onKeyDown={activateCard(() => navigate('/volunteer/visits'))}>
                <Card color="app-blue" className="finance-hero-card">
                    <div className="finance-hero-card-inner">
                        <span className="finance-hero-icon">
                            <Icon name="icon-camera" size={28} />
                        </span>
                        <h2>回访汇总</h2>
                        <p>查看已完成领养的回访记录，确保每一只猫咪在新家都过得幸福。</p>
                    </div>
                </Card>
            </div>
        </div>
    )
}