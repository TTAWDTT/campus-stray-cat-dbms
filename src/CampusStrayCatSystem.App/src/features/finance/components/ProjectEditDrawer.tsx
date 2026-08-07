import { useEffect, useState } from 'react'
import { DatePicker } from 'antd'
import dayjs from 'dayjs'
import {
    Button, Drawer, Form, useForm, FormItem, Input, Radio, Divider, Icon,
} from 'animal-island-ui'
import type { CrowdfundingProject, UpdateProjectPayload } from '../../../services/finance.service'
//本组件用于编辑众筹项目，使用了一个抽屉组件来显示表单。
const statusOptions = [
    { label: '进行中', value: 'ACTIVE' },
    { label: '已结束', value: 'COMPLETED' },
    { label: '已取消', value: 'CANCELLED' },
]

type Props = {
    open: boolean
    project: CrowdfundingProject | null
    onClose: () => void
    onSubmit: (id: string, payload: UpdateProjectPayload) => Promise<void>
}

export function ProjectEditDrawer({ open, project, onClose, onSubmit }: Props) {
    const [form] = useForm()
    const [submitting, setSubmitting] = useState(false)
    const [error, setError] = useState('')

    // 每当打开/切换项目时，回填表单
    useEffect(() => {
        if (open && project) {
            setError('')
            form.setFieldsValue({
                title: project.Title ?? '',
                catID: project.CatID ?? '',
                targetAmount: project.TargetAmount ?? '',
                startTime: project.StartTime ? dayjs(project.StartTime) : undefined,
                endTime: project.EndTime ? dayjs(project.EndTime) : undefined,
                projectStatus: project.ProjectStatus ?? 'ACTIVE',
            })
        }
    }, [open, project, form])

    const handleSubmit = async () => {
        if (!project) return
        setSubmitting(true)
        setError('')
        try {
            const values = form.getFieldsValue()
            const payload: UpdateProjectPayload = {}
            payload.ProjectID = project.ProjectID

            const title = String(values.title ?? '').trim()
            if (!title) { setError('项目标题不能为空'); setSubmitting(false); return }
            payload.Title = title

            if (values.catID != null && values.catID !== '') payload.CatID = String(values.catID)
            if (values.targetAmount !== '' && values.targetAmount != null)
                payload.TargetAmount = Number(values.targetAmount)
            if (values.startTime)
                payload.StartTime = dayjs(values.startTime as string).toISOString()
            if (values.endTime)
                payload.EndTime = dayjs(values.endTime as string).toISOString()
            if (values.projectStatus)
                payload.ProjectStatus = String(values.projectStatus) as UpdateProjectPayload['ProjectStatus']

            await onSubmit(project.ProjectID, payload)
            onClose()
        } catch (e: unknown) {
            const msg = e instanceof Error ? e.message : '更新失败'
            setError(msg)
        } finally {
            setSubmitting(false)
        }
    }

    return (
        <Drawer
            open={open}
            title="编辑项目"
            onClose={onClose}
        >
            <Form
                form={form}
                layout="vertical"
                initialValues={{
                    title: '',
                    catID: '',
                    targetAmount: '',
                    startTime: undefined,
                    endTime: undefined,
                    projectStatus: 'ACTIVE',
                }}
                onFinish={handleSubmit}
            >
                <FormItem label="项目标题" name="title" required>
                    <Input placeholder="请输入项目标题" />
                </FormItem>

                <FormItem label="关联猫咪 ID" name="catID">
                    <Input placeholder="请输入猫咪 ID（选填）" />
                </FormItem>

                <FormItem label="目标金额" name="targetAmount">
                    <Input placeholder="请输入目标金额" type="number" />
                </FormItem>

                <FormItem label="开始时间" name="startTime">
                    <DatePicker
                        style={{ width: '100%' }}
                        onChange={(date) => form.setFieldValue('startTime', date || undefined)}
                    />
                </FormItem>

                <FormItem label="结束时间" name="endTime">
                    <DatePicker
                        style={{ width: '100%' }}
                        onChange={(date) => form.setFieldValue('endTime', date || undefined)}
                    />
                </FormItem>

                <Divider type="dashed-yellow" />

                <FormItem label="项目状态" name="projectStatus" required>
                    <Radio options={statusOptions} />
                </FormItem>

                {error && (
                    <div className="cats-alert" role="alert" style={{ marginBottom: 12 }}>
                        <Icon name="icon-camera" size={17} />
                        <span>{error}</span>
                    </div>
                )}

                <FormItem>
                    <div style={{ display: 'flex', gap: 8 }}>
                        <Button type="primary" onClick={() => form.submit()} loading={submitting}>
                            保存
                        </Button>
                        <Button type="default" onClick={onClose}>
                            取消
                        </Button>
                    </div>
                </FormItem>
            </Form>
        </Drawer>
    )
}
