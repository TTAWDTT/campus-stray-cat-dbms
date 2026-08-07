import { Button, Card, Icon, Drawer, Form, FormItem, useForm, Input,Notification } from 'animal-island-ui'
import { DatePicker } from 'antd'
import { useState } from 'react'
import { financeService, type CrowdfundingProject } from '../../../services/finance.service'
import dayjs from 'dayjs'
import {useNavigate} from "react-router-dom";
import { useAuthStore } from '../../../stores/auth.store'
//本组件包含“我要捐款”和“发起众筹”两个卡片，分别用于跳转到捐款页面和打开创建众筹项目的抽屉表单。
type Props = {
    onProjectCreated?: (project: CrowdfundingProject) => void
}

export function FundCards({ onProjectCreated }: Props) {
    const [drawerOpen, setDrawerOpen] = useState(false)
    const [submitting, setSubmitting] = useState(false)
    const [error, setError] = useState('')
    const [form] = useForm()
    const user = useAuthStore((s) => s.user)
    const isAdmin=user?.roleName?.toUpperCase() === 'ADMIN'
    const navigate=useNavigate()

    const resetForm = () => {
        form.resetFields()
        setError('')
    }

    const handleClose = () => {
        resetForm()
        setDrawerOpen(false)
    }

    const handleSubmit = async () => {
        setSubmitting(true)
        setError('')
        try {
            const values = await form.validateFields()
            const targetAmount = Number(values.targetAmount)

            if (!values.title || !values.catID || !values.targetAmount || !values.startTime || !values.endTime) {
                setError('请填写所有必填字段')
                setSubmitting(false)
                return
            }
            if (values.startTime >= values.endTime) {
                setError('开始时间必须早于结束时间')
                setSubmitting(false)
                return
            }
            if (targetAmount <= 0) {
                setError('目标金额必须大于0')
                setSubmitting(false)
                return
            }

            const catExists = await financeService.catExists(String(values.catID))
            if (!catExists) {
                setError('关联的猫咪不存在，请检查猫咪 ID')
                setSubmitting(false)
                return
            }

            const payload = {
                Title: String(values.title).trim(),
                CatID: String(values.catID).trim(),
                TargetAmount: targetAmount,
                StartTime: dayjs(values.startTime as unknown as string | Date).toISOString(),
                EndTime: dayjs(values.endTime as unknown as string | Date).toISOString(),
                ProjectStatus: 'ACTIVE' as const,
            }
            const created = await financeService.createProject(payload)
            onProjectCreated?.(created)
            handleClose()
        } catch (e: unknown) {
            const msg = e instanceof Error ? e.message : '创建项目失败，请稍后重试'
            setError(msg)
        } finally {
            setSubmitting(false)
        }
    }

    return (
        <div className="finance-hero-grid">
            <Card color="app-teal" className="finance-hero-card">
                <div className="finance-hero-card-inner">
                    <span className="finance-hero-icon">
                        <Icon name="icon-design" size={28} />
                    </span>
                    <h2>发起众筹</h2>
                    <p>为校园猫咪发起一个新的众筹项目，设定目标金额和截止日期，让更多人参与帮助。</p>
                    <Button type="default" size="middle" onClick={() => {
                        if (isAdmin) {
                            setDrawerOpen(true)
                        } else {
                            Notification.error('权限不足：管理员才能发起众筹项目')
                        }
                    }}>
                        <span>🙋‍♂️ 发起众筹</span>
                    </Button>
                </div>
            </Card>
            <Card color="app-blue" className="finance-hero-card">
                <div className="finance-hero-card-inner">
                    <span className="finance-hero-icon">
                        <Icon name="icon-critterpedia" size={28} />
                    </span>
                    <h2>我要捐款</h2>
                    <p>选择你关心的猫咪或项目，献出一份爱心。你的每一笔捐赠都将透明公示。</p>
                    <Button type="default" size="middle" onClick={()=>navigate('/finance/projects')}>
                        <span>❤️ 我要捐款</span>
                    </Button>
                </div>
            </Card>

            <Drawer open={drawerOpen} onClose={handleClose} title="发起众筹项目" width={480}>
                <Form form={form} layout="vertical" onFinish={handleSubmit}>
                    <FormItem label="关联猫咪 ID" name="catID" required>
                        <Input placeholder="请输入猫咪 ID" />
                    </FormItem>
                    <FormItem label="项目标题" name="title" required>
                        <Input placeholder="请输入项目标题" />
                    </FormItem>
                    <FormItem label="目标金额" name="targetAmount" required>
                        <Input placeholder="请输入目标金额" type="number" />
                    </FormItem>
                    <FormItem label="开始时间" name="startTime" required>
                        <DatePicker
                            style={{ width: '100%' }}
                            onChange={(date) => form.setFieldValue('startTime', date || undefined)}
                        />
                    </FormItem>
                    <FormItem label="结束时间" name="endTime" required>
                        <DatePicker
                            style={{ width: '100%' }}
                            onChange={(date) => form.setFieldValue('endTime', date || undefined)}
                        />
                    </FormItem>

                    {error && (
                        <div className="cats-alert" role="alert" style={{ marginBottom: 12 }}>
                            <Icon name="icon-camera" size={17} />
                            <span>{error}</span>
                        </div>
                    )}

                    <FormItem>
                        <div style={{ display: 'flex', gap: 8 }}>
                            <Button type="primary" size="middle" onClick={() => form.submit()} loading={submitting}>
                                <span>提交</span>
                            </Button>
                            <Button type="default" size="middle" onClick={handleClose}>
                                <span>取消</span>
                            </Button>
                        </div>
                    </FormItem>
                </Form>
            </Drawer>
        </div>
    )
}