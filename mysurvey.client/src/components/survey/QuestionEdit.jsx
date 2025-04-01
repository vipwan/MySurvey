import React, { useState, useEffect } from 'react';
import { Form, Input, Select, Switch, Button, Space, Card, InputNumber, message, Divider, Alert } from 'antd';
import { PlusOutlined, MinusCircleOutlined, InfoCircleOutlined } from '@ant-design/icons';

const { Option } = Select;
const { TextArea } = Input;

// 定义问题类型常量，与后端枚举保持一致
const QuestionType = {
    SingleChoice: 0,
    MultipleChoice: 1,
    TextInput: 2,
    Rating: 3,
    MatrixSingleChoice: 4,
    MatrixMultipleChoice: 5
};

// 定义验证规则类型，与后端枚举保持一致
const ValidationRuleType = {
    None: 0,
    PhoneNumber: 1,
    IdCard: 2,
    Email: 3,
    Custom: 4
};

const QuestionEdit = ({ question, onSave, onCancel }) => {
    const [form] = Form.useForm();
    const [questionType, setQuestionType] = useState(question?.type !== undefined ? question.type : QuestionType.TextInput);
    const [validationRuleType, setValidationRuleType] = useState(question?.validationRuleType || ValidationRuleType.None);
    const [customValidationVisible, setCustomValidationVisible] = useState(
        question?.validationRuleType === ValidationRuleType.Custom
    );

    // 当组件加载或question改变时，确保表单值和state同步
    useEffect(() => {
        if (question) {
            setQuestionType(question.type !== undefined ? question.type : QuestionType.TextInput);
            setValidationRuleType(question.validationRuleType || ValidationRuleType.None);
            setCustomValidationVisible(question.validationRuleType === ValidationRuleType.Custom);

            form.setFieldsValue({
                ...question,
                type: question.type, // 确保type字段被正确设置
                validationRuleType: question.validationRuleType || ValidationRuleType.None,
                customValidationPattern: question.customValidationPattern || '',
                validationErrorMessage: question.validationErrorMessage || ''
            });
        }
    }, [question, form]);

    // 尝试从选项中获取评分的最小值和最大值
    const getInitialRatingValues = () => {
        if (question?.type === QuestionType.Rating && question.options && question.options.length >= 2) {
            return {
                ratingMin: parseInt(question.options[0].value) || 1,
                ratingMax: parseInt(question.options[1].value) || 5
            };
        }
        return { ratingMin: 1, ratingMax: 5 };
    };

    const { ratingMin: initialRatingMin, ratingMax: initialRatingMax } = getInitialRatingValues();

    // 处理问题类型变更
    const handleTypeChange = (value) => {
        console.log('问题类型变更为:', value);
        setQuestionType(Number(value)); // 确保转换为数字

        // 如果切换到单选或多选，且没有选项，则添加一个默认选项
        if ((value === QuestionType.SingleChoice || value === QuestionType.MultipleChoice) &&
            (!form.getFieldValue('options') || form.getFieldValue('options').length === 0)) {
            form.setFieldsValue({
                options: [{ content: '', order: 0, isDefault: false, value: '' }]
            });
        }

        // 如果不是填空题，清除验证规则
        if (value !== QuestionType.TextInput) {
            form.setFieldsValue({
                validationRuleType: ValidationRuleType.None,
                customValidationPattern: '',
                validationErrorMessage: ''
            });
            setValidationRuleType(ValidationRuleType.None);
            setCustomValidationVisible(false);
        }
    };

    // 处理验证规则类型变更
    const handleValidationRuleTypeChange = (value) => {
        setValidationRuleType(Number(value));
        setCustomValidationVisible(Number(value) === ValidationRuleType.Custom);

        // 根据验证类型设置默认的错误提示信息
        let defaultErrorMessage = '';
        switch (Number(value)) {
            case ValidationRuleType.PhoneNumber:
                defaultErrorMessage = '请输入正确的手机号码';
                break;
            case ValidationRuleType.IdCard:
                defaultErrorMessage = '请输入正确的身份证号码';
                break;
            case ValidationRuleType.Email:
                defaultErrorMessage = '请输入正确的邮箱地址';
                break;
            case ValidationRuleType.Custom:
                defaultErrorMessage = '输入格式不正确';
                break;
            default:
                defaultErrorMessage = '';
        }

        form.setFieldsValue({
            validationErrorMessage: defaultErrorMessage
        });
    };

    // 单选题默认选中选项的处理
    const handleDefaultChange = (checked, currentIndex) => {
        // 如果是单选题，则只允许一个选项被默认选中
        if (questionType === QuestionType.SingleChoice && checked) {
            // 获取当前所有选项
            const allOptions = form.getFieldValue('options');

            // 清除其他选项的默认选中状态
            allOptions.forEach((_, index) => {
                if (index !== currentIndex) {
                    form.setFieldValue(['options', index, 'isDefault'], false);
                }
            });
        }
    };

    // 修复填空题验证规则绑定问题
    const handleSubmit = () => {
        form.validateFields().then(values => {
            // 确保类型值是数字
            values.type = Number(values.type);

            // 确保验证规则类型为数字，默认为None(0)
            values.validationRuleType = Number(values.validationRuleType || ValidationRuleType.None);

            if (values.type === QuestionType.Rating) {
                // 评分题处理部分保持不变...
                // ...
            } else if (values.type === QuestionType.SingleChoice || values.type === QuestionType.MultipleChoice) {
                // 单选题和多选题处理部分保持不变...
                // ...
            } else if (values.type === QuestionType.TextInput) {
                // 填空题处理
                values.options = [];

                // 确保验证规则类型正确设置
                if (values.validationRuleType === undefined || values.validationRuleType === null) {
                    values.validationRuleType = ValidationRuleType.None;
                }

                // 如果不是自定义验证，清空自定义验证规则
                if (values.validationRuleType !== ValidationRuleType.Custom) {
                    values.customValidationPattern = '';
                } else if (!values.customValidationPattern) {
                    // 确保自定义验证有验证规则
                    message.error('请输入自定义验证规则');
                    return;
                }

                // 如果无验证规则，清空错误提示
                if (values.validationRuleType === ValidationRuleType.None) {
                    values.validationErrorMessage = '';
                } else if (!values.validationErrorMessage) {
                    // 确保有验证规则时有错误提示
                    message.error('请输入验证失败提示信息');
                    return;
                }
            } else {
                // 其他类型问题处理
                values.options = [];
                values.validationRuleType = ValidationRuleType.None;
                values.customValidationPattern = '';
                values.validationErrorMessage = '';
            }

            console.log('Question data being saved:', values);
            onSave(values);
        });
    };


    // 获取验证规则类型的说明文字
    const getValidationRuleDescription = (type) => {
        switch (type) {
            case ValidationRuleType.PhoneNumber:
                return '将验证用户输入是否符合中国大陆手机号格式（11位数字，以1开头）';
            case ValidationRuleType.IdCard:
                return '将验证用户输入是否符合中国身份证号格式（15位或18位）';
            case ValidationRuleType.Email:
                return '将验证用户输入是否符合电子邮件地址格式';
            case ValidationRuleType.Custom:
                return '使用自定义正则表达式验证用户输入';
            default:
                return '';
        }
    };

    return (
        <Form
            form={form}
            layout="vertical"
            initialValues={{
                ...question,
                type: question?.type !== undefined ? question.type : QuestionType.TextInput,
                isRequired: question?.isRequired ?? true,
                options: question?.options || [{ content: '', order: 0, isDefault: false }],
                ratingMin: initialRatingMin,
                ratingMax: initialRatingMax,
                validationRuleType: question?.validationRuleType || ValidationRuleType.None,
                customValidationPattern: question?.customValidationPattern || '',
                validationErrorMessage: question?.validationErrorMessage || ''
            }}
        >
            <Form.Item
                name="title"
                label="问题标题"
                rules={[{ required: true, message: '请输入问题标题' }]}
            >
                <Input placeholder="请输入问题标题" />
            </Form.Item>

            <Form.Item
                name="description"
                label="问题描述"
            >
                <TextArea rows={4} placeholder="请输入问题描述" />
            </Form.Item>

            <Form.Item
                name="type"
                label="问题类型"
                rules={[{ required: true, message: '请选择问题类型' }]}
            >
                <Select onChange={handleTypeChange}>
                    <Option value={QuestionType.SingleChoice}>单选题</Option>
                    <Option value={QuestionType.MultipleChoice}>多选题</Option>
                    <Option value={QuestionType.TextInput}>填空题</Option>
                    <Option value={QuestionType.Rating}>评分题</Option>
                </Select>
            </Form.Item>

            <Form.Item
                name="isRequired"
                label="是否必答"
                valuePropName="checked"
            >
                <Switch />
            </Form.Item>

            {/* 填空题验证规则配置 */}
            {questionType === QuestionType.TextInput && (
                <>
                    <Divider orientation="left">答案验证规则</Divider>

                    <Form.Item
                        name="validationRuleType"
                        label="验证规则类型"
                    >
                        <Select onChange={handleValidationRuleTypeChange}>
                            <Option value={ValidationRuleType.None}>无验证</Option>
                            <Option value={ValidationRuleType.PhoneNumber}>手机号码</Option>
                            <Option value={ValidationRuleType.IdCard}>身份证号码</Option>
                            <Option value={ValidationRuleType.Email}>电子邮件</Option>
                            <Option value={ValidationRuleType.Custom}>自定义正则表达式</Option>
                        </Select>
                    </Form.Item>

                    {validationRuleType !== ValidationRuleType.None && (
                        <Alert
                            style={{ marginBottom: 16 }}
                            message={getValidationRuleDescription(validationRuleType)}
                            type="info"
                            showIcon
                            icon={<InfoCircleOutlined />}
                        />
                    )}

                    {customValidationVisible && (
                        <Form.Item
                            name="customValidationPattern"
                            label="自定义验证规则"
                            tooltip="请输入标准的正则表达式，如：^[0-9]{6}$ 表示6位数字"
                            rules={[{ required: customValidationVisible, message: '请输入自定义验证规则' }]}
                        >
                            <Input placeholder="输入正则表达式" />
                        </Form.Item>
                    )}

                    {validationRuleType !== ValidationRuleType.None && (
                        <Form.Item
                            name="validationErrorMessage"
                            label="验证失败提示信息"
                            rules={[{ required: validationRuleType !== ValidationRuleType.None, message: '请输入验证失败提示信息' }]}
                        >
                            <Input placeholder="当答案不符合验证规则时显示的提示信息" />
                        </Form.Item>
                    )}
                </>
            )}

            {(questionType === QuestionType.SingleChoice || questionType === QuestionType.MultipleChoice) && (
                <Form.List name="options">
                    {(fields, { add, remove }) => (
                        <>
                            {fields.map(({ key, name, ...restField }, index) => (
                                <Card key={key} style={{ marginBottom: 16 }}>
                                    <Space direction="vertical" style={{ width: '100%' }}>
                                        <Space align="baseline">
                                            <Form.Item
                                                {...restField}
                                                name={[name, 'content']}
                                                rules={[{ required: true, message: '请输入选项内容' }]}
                                            >
                                                <Input placeholder="选项内容" />
                                            </Form.Item>
                                            <Form.Item
                                                {...restField}
                                                name={[name, 'order']}
                                                rules={[{ required: true, message: '请输入选项顺序' }]}
                                            >
                                                <InputNumber min={0} placeholder="顺序" />
                                            </Form.Item>
                                            <Form.Item
                                                {...restField}
                                                name={[name, 'value']}
                                            >
                                                <Input placeholder="选项值（可选）" />
                                            </Form.Item>
                                            <MinusCircleOutlined onClick={() => remove(name)} />
                                        </Space>
                                        <Form.Item
                                            {...restField}
                                            name={[name, 'isDefault']}
                                            valuePropName="checked"
                                            label="默认选中"
                                        >
                                            <Switch
                                                onChange={(checked) => handleDefaultChange(checked, index)}
                                            />
                                        </Form.Item>
                                    </Space>
                                </Card>
                            ))}
                            <Form.Item>
                                <Button
                                    type="dashed"
                                    onClick={() => add({ content: '', order: fields.length, isDefault: false, value: '' })}
                                    block
                                    icon={<PlusOutlined />}
                                >
                                    添加选项
                                </Button>
                            </Form.Item>
                        </>
                    )}
                </Form.List>
            )}

            {questionType === QuestionType.Rating && (
                <>
                    <Form.Item
                        name="ratingMin"
                        label="最小分值"
                        rules={[
                            {
                                required: questionType === QuestionType.Rating,
                                message: '请输入最小分值'
                            }
                        ]}
                    >
                        <InputNumber min={0} max={9} />
                    </Form.Item>
                    <Form.Item
                        name="ratingMax"
                        label="最大分值"
                        rules={[
                            {
                                required: questionType === QuestionType.Rating,
                                message: '请输入最大分值'
                            },
                            ({ getFieldValue }) => ({
                                validator(_, value) {
                                    if (!value || getFieldValue('ratingMin') < value) {
                                        return Promise.resolve();
                                    }
                                    return Promise.reject(new Error('最大分值必须大于最小分值'));
                                },
                            }),
                        ]}
                    >
                        <InputNumber min={1} max={10} />
                    </Form.Item>
                </>
            )}

            <Form.Item>
                <Space>
                    <Button type="primary" onClick={handleSubmit}>
                        保存
                    </Button>
                    <Button onClick={onCancel}>
                        取消
                    </Button>
                </Space>
            </Form.Item>
        </Form>
    );
};

export default QuestionEdit;
