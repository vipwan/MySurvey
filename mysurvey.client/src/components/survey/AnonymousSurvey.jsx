import React, { useState } from 'react';
import { Card, Form, Button, message, Spin, Result, Skeleton, Input, Radio, Checkbox, Rate, Table, Space } from 'antd';
import { useParams, useNavigate } from 'react-router-dom';
import { useRequest, useTitle } from 'ahooks';
import { surveyApi } from '../../services/api';

// 问题类型枚举（与后端QuestionType保持一致）
const QuestionType = {
    SingleChoice: 0,      // 单选题
    MultipleChoice: 1,    // 多选题
    TextInput: 2,         // 填空题
    Rating: 3,            // 评分题
    MatrixSingleChoice: 4, // 矩阵单选题
    MatrixMultipleChoice: 5 // 矩阵多选题
};

// 矩阵问题组件
const MatrixQuestion = ({ question, type, onChange }) => {
    // 获取行和列
    const rows = question.options.filter(opt => opt.isMatrixRow);
    const columns = question.options.filter(opt => opt.isMatrixColumn);

    // 矩阵数据状态
    const [matrixValues, setMatrixValues] = useState({});

    // 处理单元格选择变化
    const handleCellChange = (rowId, columnId, value) => {
        const newValues = {
            ...matrixValues,
            [`${rowId}_${columnId}`]: value
        };
        setMatrixValues(newValues);

        // 转换为API所需的格式
        const matrixAnswers = Object.entries(newValues).map(([key, val]) => {
            const [rowId, columnId] = key.split('_');
            return {
                rowId,
                columnId,
                value: val.toString()
            };
        });

        onChange(matrixAnswers);
    };

    // 列定义
    const columns_ = [
        {
            title: '选项/评分',
            dataIndex: 'rowContent',
            key: 'rowContent',
            width: '30%'
        },
        ...columns.map(col => ({
            title: col.content,
            dataIndex: col.id,
            key: col.id,
            render: (_, record) => {
                // 根据不同矩阵类型渲染不同控件
                if (type === QuestionType.MatrixSingleChoice) {
                    return (
                        <Radio.Group
                            onChange={(e) => handleCellChange(record.rowId, col.id, e.target.value)}
                            value={matrixValues[`${record.rowId}_${col.id}`]}
                        >
                            <Radio value={col.id} />
                        </Radio.Group>
                    );
                } else if (type === QuestionType.MatrixMultipleChoice) {
                    return (
                        <Checkbox
                            onChange={(e) => handleCellChange(record.rowId, col.id, e.target.checked)}
                            checked={matrixValues[`${record.rowId}_${col.id}`]}
                        />
                    );
                }
                return null;
            }
        }))
    ];

    // 行数据
    const dataSource = rows.map(row => ({
        key: row.id,
        rowId: row.id,
        rowContent: row.content,
    }));

    return (
        <Table
            dataSource={dataSource}
            columns={columns_}
            pagination={false}
            bordered
            size="middle"
        />
    );
};

const AnonymousSurvey = () => {
    const { id } = useParams();
    const navigate = useNavigate();
    const [form] = Form.useForm();
    const [submitted, setSubmitted] = useState(false);

    // 使用 ahooks 的 useRequest 替代 useEffect 获取数据
    const { data: rawData, loading } = useRequest(
        () => surveyApi.getSurvey(id),
        {
            onSuccess: (response) => {
                // 成功获取数据后，设置默认值
                if (response?.data?.questions) {
                    // 延迟执行，确保表单已经渲染
                    setTimeout(() => {
                        const initialValues = {};
                        response.data.questions.forEach(question => {
                            if (question.type === QuestionType.SingleChoice) {
                                // 单选题默认值
                                const defaultOption = question.options.find(o => o.isDefault);
                                if (defaultOption) {
                                    initialValues[`question_${question.id}`] = defaultOption.id;
                                }
                            } else if (question.type === QuestionType.MultipleChoice) {
                                // 多选题默认值
                                const defaultOptions = question.options.filter(o => o.isDefault);
                                if (defaultOptions.length > 0) {
                                    initialValues[`question_${question.id}`] = defaultOptions.map(o => o.id);
                                }
                            }
                        });

                        // 批量设置默认值
                        if (Object.keys(initialValues).length > 0) {
                            form.setFieldsValue(initialValues);
                        }
                    }, 100);
                }
            },
            onError: (error) => {
                console.error('获取问卷错误:', error);
                message.error('获取问卷失败');
            }
        }
    );

    // 提取响应数据
    const survey = rawData?.data;

    useTitle(survey?.title ?? '');


    // 使用 useRequest 处理表单提交
    const { run: submitSurvey, loading: submitting } = useRequest(
        (values) => surveyApi.submitAnswer(id, values),
        {
            manual: true,
            onSuccess: () => {
                message.success('提交成功！感谢您的参与。');
                setSubmitted(true);
            },
            onError: (error) => {
                console.error('提交错误:', error);
                message.error('提交失败，请重试');
            }
        }
    );

    const handleSubmit = (values) => {
        // 将表单数据转换成API需要的格式
        const questionAnswers = [];

        // 处理表单数据
        Object.entries(values).forEach(([key, value]) => {
            if (key.startsWith('question_')) {
                const questionId = key.replace('question_', '');
                const question = survey.questions.find(q => q.id === questionId);

                if (!question) return;

                let answerData = {
                    questionId,
                    content: null,
                    optionIds: null,
                    optionValues: null,
                    ratingValue: null,
                    matrixAnswers: null
                };

                // 根据问题类型处理不同的答案格式
                switch (question.type) {
                    case QuestionType.TextInput:
                        answerData.content = value;
                        break;

                    case QuestionType.SingleChoice:
                        answerData.optionIds = [value];
                        break;

                    case QuestionType.MultipleChoice:
                        answerData.optionIds = value || [];
                        break;

                    case QuestionType.Rating:
                        answerData.ratingValue = value;
                        break;

                    case QuestionType.MatrixSingleChoice:
                    case QuestionType.MatrixMultipleChoice:
                        answerData.matrixAnswers = value;
                        break;
                }

                questionAnswers.push(answerData);
            }
        });

        const formattedValues = {
            anonymousId: null, // 或者可以生成一个匿名ID
            questionAnswers: questionAnswers
        };

        submitSurvey(formattedValues);
    };

    // 显示加载状态
    if (loading) {
        return (
            <>
                <Skeleton/>
                <div style={{ textAlign: 'center', padding: '50px' }}>
                    <Spin size="large" />
                </div>
            </>

        );
    }

    // 问卷不存在
    if (!survey) {
        return (
            <Result
                status="error"
                title="问卷不存在"
                subTitle="该问卷可能已被删除或未发布"
                extra={
                    <Button type="primary" onClick={() => navigate('/')}>
                        返回首页
                    </Button>
                }
            />
        );
    }

    // 提交成功
    if (submitted) {
        return (
            <Result
                status="success"
                title="提交成功"
                subTitle="感谢您的参与！"
                extra={
                    <Button type="primary" onClick={() => navigate('/')}>
                        返回首页
                    </Button>
                }
            />
        );
    }

    // 为单选题和多选题预设默认值
    const getDefaultValueProps = (question) => {
        if (question.type === QuestionType.SingleChoice) {
            const defaultOption = question.options.find(opt => opt.isDefault);
            return defaultOption ? { defaultValue: defaultOption.id } : {};
        } else if (question.type === QuestionType.MultipleChoice) {
            const defaultOptions = question.options.filter(opt => opt.isDefault);
            return defaultOptions.length > 0 ? { defaultValue: defaultOptions.map(opt => opt.id) } : {};
        }
        return {};
    };

    // 渲染不同类型的问题控件
    const renderQuestionControl = (question) => {
        switch (question.type) {
            case QuestionType.TextInput:
                return <Input.TextArea rows={4} />;

            case QuestionType.SingleChoice:
                return (
                    <Radio.Group>
                        {question.options?.map(option => (
                            <Radio key={option.id} value={option.id}>
                                {option.content}
                            </Radio>
                        ))}
                    </Radio.Group>
                );

            case QuestionType.MultipleChoice:
                return (
                    <Checkbox.Group>
                        {question.options?.map(option => (
                            <Checkbox key={option.id} value={option.id}>
                                {option.content}
                            </Checkbox>
                        ))}
                    </Checkbox.Group>
                );

            case QuestionType.Rating:
                // 根据选项设置Rate的总数
                const maxRate = question.options && question.options.length > 0 ?
                    Math.max(...question.options.map(o => parseInt(o.value) || 5)) : 5;

                return <Rate count={maxRate} />;

            case QuestionType.MatrixSingleChoice:
            case QuestionType.MatrixMultipleChoice:
                return (
                    <Form.Item noStyle>
                        {({ value, onChange }) => (
                            <MatrixQuestion
                                question={question}
                                type={question.type}
                                onChange={onChange}
                                value={value}
                            />
                        )}
                    </Form.Item>
                );

            default:
                return <div>不支持的问题类型: {question.type}</div>;
        }
    };

    // 检查问题列表是否存在且非空
    const hasQuestions = Array.isArray(survey.questions) && survey.questions.length > 0;

    return (
        <div style={{ maxWidth: 800, margin: '0 auto', padding: '20px' }}>
            <Card
                title={survey.title}
                extra={survey.description && <div>{survey.description}</div>}
            >
                <Form
                    form={form}
                    layout="vertical"
                    onFinish={handleSubmit}
                >
                    {hasQuestions ? (
                        survey.questions.map((question, index) => (
                            <Form.Item
                                key={question.id}
                                label={
                                    <Space>
                                        <span>{`${index + 1}. ${question.title}`}</span>
                                        {question.isRequired && <span style={{ color: 'red' }}>*</span>}
                                    </Space>
                                }
                                name={`question_${question.id}`}
                                rules={[{ required: question.isRequired, message: '请回答此问题' }]}
                                extra={question.description}
                                {...getDefaultValueProps(question)}
                            >
                                {renderQuestionControl(question)}
                            </Form.Item>
                        ))
                    ) : (
                        <div>
                            <p>此问卷暂无问题</p>
                            <p>问卷ID: {survey.id}</p>
                            <p>问卷标题: {survey.title}</p>
                            <p>问卷状态: {survey.status}</p>
                        </div>
                    )}
                    <Form.Item>
                        <Button type="primary" htmlType="submit" loading={submitting}>
                            提交问卷
                        </Button>
                    </Form.Item>
                </Form>
            </Card>
        </div>
    );
};

export default AnonymousSurvey;
