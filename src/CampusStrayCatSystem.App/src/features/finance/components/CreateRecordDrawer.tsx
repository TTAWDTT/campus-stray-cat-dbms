import { useEffect, useState } from 'react'
import { DatePicker } from 'antd'
import dayjs from 'dayjs'
import {
    Button, Drawer, Form, useForm, FormItem, Input, Radio, Icon,
} from 'animal-island-ui'
import type { CreateExpensePayload, CreateDonationPayload } from '../../../services/finance.service'
//本组件用于创建支出记录和捐款记录，使用了一个抽屉组件来显示表单。根据传入的 activeKey 参数，决定是创建支出记录还是捐款记录。
// 表单选项 
const radioTypeOptions = [
    { label: '🍔 食物', value: 'FOOD' },
    { label: '🚑 医疗', value: 'MEDICAL' },
    { label: '🧺 物资', value: 'SUPPLIES' },
    { label: '❓ 其他', value: 'OTHER' },
]

const payTypeOptions = [
    { label: '支付宝', value: 'ALIPAY' },
    { label: '微信', value: 'WECHAT' },
    { label: '银行转账', value: 'BANK_TRANSFER' },
    { label: '现金', value: 'CASH' },
    { label: '其他', value: 'OTHER' },
]

const pubOptions = [
    { label: '公开', value: 1 },
    { label: '匿名', value: 0 },
]

type Props = {
    open: boolean
    activeKey: string // 'payment' | 'donation'
    onClose: () => void
    onCreateExpense: (payload: CreateExpensePayload) => Promise<void>
    onCreateDonation: (payload: CreateDonationPayload) => Promise<void>
    /** 锁定项目 ID，字段预填且不可修改 */
    lockedProjectID?: string
    /** 锁定捐赠人 ID，字段预填且不可修改 */
    lockedDonorUserID?: string
}

const expenseInit = { projectID: '', recordType: 'FOOD', amount: 0, invoiceUrl: '' }
const donationInit = { projectID: '', donorUserID: '', amount: 0, payMethod: '', payTime: undefined as dayjs.Dayjs | undefined, publicFlag: 0 }

export function CreateRecordDrawer({ open, activeKey, onClose, onCreateExpense, onCreateDonation, lockedProjectID, lockedDonorUserID }: Props) {
    const [form] = useForm()
    const [submitting, setSubmitting] = useState(false)
    const [error, setError] = useState('')

    const isPayment = activeKey === 'payment'

    // 每次打开/切换 tab 时重置表单，并应用锁定值
    useEffect(() => {
        if (open) {
            setError('')
            const base = isPayment ? expenseInit : { ...donationInit, payTime: dayjs() }
            form.setFieldsValue({
                ...base,
                ...(lockedProjectID ? { projectID: lockedProjectID } : {}),
                ...(lockedDonorUserID ? { donorUserID: lockedDonorUserID } : {}),
            })
        }
    }, [open, isPayment, form, lockedProjectID, lockedDonorUserID])

    const handleSubmit = async () => {
        setSubmitting(true)
        setError('')
        try {
            const values = form.getFieldsValue()
            if (isPayment) {
                const payload: CreateExpensePayload = {
                    ProjectID: String(values.projectID ?? ''),
                    RecordType: String(values.recordType ?? 'FOOD'),
                    Amount: Number(values.amount) || 0,
                    InvoiceUrl: String(values.invoiceUrl ?? ''),
                }
                if (!payload.ProjectID) { setError('项目ID 不能为空'); setSubmitting(false); return }
                if (!payload.Amount || payload.Amount <= 0) { setError('金额必须大于 0'); setSubmitting(false); return }
                await onCreateExpense(payload)
            } else {
                const payload: CreateDonationPayload = {
                    ProjectID: String(values.projectID ?? ''),
                    DonorUserID: values.donorUserID ? String(values.donorUserID) : undefined,
                    Amount: Number(values.amount) || 0,
                    PayMethod: values.payMethod ? String(values.payMethod) : undefined,
                    PayTime: values.payTime ? dayjs(values.payTime as string).toISOString() : undefined,
                    PublicFlag: values.publicFlag != null ? Number(values.publicFlag) : 0,
                }
                if (!payload.ProjectID) { setError('项目ID 不能为空'); setSubmitting(false); return }
                if (!payload.Amount || payload.Amount <= 0) { setError('金额必须大于 0'); setSubmitting(false); return }
                await onCreateDonation(payload)
            }
            onClose()
            form.resetFields()
        } finally {
            setSubmitting(false)
        }
    }

    return (
        <Drawer
            open={open}
            title={`新建${isPayment ? '支出记录' : '捐款记录'}`}
            onClose={onClose}
        >
            <Form
                form={form}
                layout="vertical"
                initialValues={isPayment ? expenseInit : donationInit}
                onFinish={handleSubmit}
            >
                <FormItem label="项目 ID" name="projectID" required>
                    <Input placeholder="请输入对应项目 ID" disabled={!!lockedProjectID} />
                </FormItem>

                {isPayment ? (
                    <>
                        <FormItem label="记录类型" name="recordType" required>
                            <Radio options={radioTypeOptions} />
                        </FormItem>
                        <FormItem label="金额" name="amount" required>
                            <Input placeholder="请输入金额" type="number" />
                        </FormItem>
                        <FormItem label="发票链接" name="invoiceUrl" required>
                            <Input placeholder="请输入发票链接" />
                        </FormItem>
                    </>
                ) : (
                    <>
                        <FormItem label="捐赠人 ID" name="donorUserID">
                            <Input placeholder="请输入捐赠人 ID" disabled={!!lockedDonorUserID} />
                        </FormItem>
                        <FormItem label="金额" name="amount" required>
                            <Input placeholder="请输入金额" type="number" />
                        </FormItem>
                        <FormItem label="支付方式" name="payMethod">
                            <Radio options={payTypeOptions} />
                        </FormItem>
                        <FormItem label="支付时间" name="payTime">
                            <DatePicker onChange={(date) => form.setFieldValue('payTime', date || undefined)} />
                        </FormItem>
                        <FormItem label="是否公开" name="publicFlag">
                            <Radio options={pubOptions} />
                        </FormItem>
                    </>
                )}

                {error && (
                    <div className="cats-alert" role="alert" style={{ marginBottom: 12 }}>
                        <Icon name="icon-camera" size={17} />
                        <span>{error}</span>
                    </div>
                )}

                <FormItem>
                    <div style={{ display: 'flex', gap: 8 }}>
                        <Button type="primary" onClick={() => form.submit()} loading={submitting}>
                            提交
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
