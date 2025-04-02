/*
 * @Author: 万雅虎
 * @Date: 2025-03-29 20:47:21
 * @LastEditTime: 2025-04-01 23:38:14
 * @LastEditors: 万雅虎
 * @Description: 
 * @FilePath: \MySurvey\mysurvey.client\src\components\survey\QuestionList.jsx
 * vipwan@sina.com © 万雅虎
 */
import React from 'react';
import { Button, Space, message, Drawer, Popconfirm, Typography, Tooltip, Form, InputNumber, Card } from 'antd';
import { PlusOutlined, EditOutlined, DeleteOutlined, CopyOutlined, OrderedListOutlined } from '@ant-design/icons';
import { useReactive,useMount } from 'ahooks';
import { ProTable } from '@ant-design/pro-components'

import { useParams } from 'react-router-dom';
import { surveyApi } from '../../services/api';
import QuestionEdit from './QuestionEdit';

const { Title } = Typography;

// 定义问题类型常量，与后端枚举保持一致
const QuestionType = {
    SingleChoice: 0,
    MultipleChoice: 1,
    TextInput: 2,
    Rating: 3,
    MatrixSingleChoice: 4,
    MatrixMultipleChoice: 5
};

const QuestionList = () => {
    const { id: surveyId } = useParams();
    const [form] = Form.useForm();

    // 使用 useReactive 替代多个 useState
    const state = useReactive({
        questions: [],
        loading: false,
        drawerVisible: false,
        sortDrawerVisible: false,
        editingQuestion: null,
        isCreating: false,
        survey: null,
        sortLoading: false
    });

    // 获取问卷信息和问题列表
    const fetchData = async () => {
        if (!surveyId) {
            console.error('surveyId is undefined in fetchData');
            message.error('问卷ID获取失败');
            return;
        }
        state.loading = true;
        try {
            console.log('Fetching survey with ID:', surveyId);
            const response = await surveyApi.getSurvey(surveyId);
            const surveyData = response.data;
            state.survey = surveyData;
            state.questions = surveyData.questions || [];
        } catch (error) {
            console.error('获取问题列表失败:', error);
            message.error('获取问题列表失败');
        } finally {
            state.loading = false;
        }
    };

    useMount(() => {
        if (surveyId) {
            fetchData();
        } else {
            console.error('surveyId is undefined in useEffect');
            message.error('问卷ID获取失败');
        }
    });

    // 添加新问题
    const handleAddQuestion = () => {
        console.log('Adding question for surveyId:', surveyId);
        if (!surveyId) {
            console.error('surveyId is undefined in handleAddQuestion');
            message.error('问卷ID获取失败');
            return;
        }
        state.isCreating = true;
        state.editingQuestion = {
            title: '',
            description: '',
            type: QuestionType.TextInput,
            isRequired: true,
            order: state.questions.length,
            options: []
        };
        state.drawerVisible = true;
    };

    // 编辑问题
    const handleEditQuestion = (question) => {
        if (!surveyId) {
            message.error('问卷ID不能为空');
            return;
        }
        if (!question?.id) {
            message.error('问题ID不能为空');
            return;
        }
        state.isCreating = false;
        state.editingQuestion = question;
        state.drawerVisible = true;
    };

    // 复制问题
    const handleCopyQuestion = async (question) => {
        try {
            const copiedQuestion = {
                ...question,
                title: `${question.title} (复制)`,
                order: state.questions.length,
                id: undefined
            };

            // 准备选项数据
            let processedOptions = [];

            // 根据问题类型处理选项
            if (copiedQuestion.type === QuestionType.SingleChoice ||
                copiedQuestion.type === QuestionType.MultipleChoice) {
                // 单选题和多选题需要处理选项
                const options = copiedQuestion.options || [];
                processedOptions = options.map(option => ({
                    content: option.content,
                    value: option.value || option.content,
                    order: option.order,
                    isDefault: option.isDefault || false,
                    isMatrixRow: option.isMatrixRow || false,
                    isMatrixColumn: option.isMatrixColumn || false
                }));
            } else if (copiedQuestion.type === QuestionType.Rating) {
                // 对于评分题，保留原始的最小值和最大值
                const options = copiedQuestion.options || [];
                let ratingMin = 1;
                let ratingMax = 5;

                if (options.length >= 2) {
                    ratingMin = parseInt(options[0].value) || 1;
                    ratingMax = parseInt(options[1].value) || 5;
                }

                // 创建两个选项来表示评分范围
                processedOptions = [
                    {
                        content: `${ratingMin}分`,
                        value: `${ratingMin}`,
                        order: 0,
                        isDefault: false,
                        isMatrixRow: false,
                        isMatrixColumn: false
                    },
                    {
                        content: `${ratingMax}分`,
                        value: `${ratingMax}`,
                        order: 1,
                        isDefault: false,
                        isMatrixRow: false,
                        isMatrixColumn: false
                    }
                ];
            }

            const requestData = {
                title: copiedQuestion.title,
                description: copiedQuestion.description,
                type: copiedQuestion.type,
                isRequired: copiedQuestion.isRequired,
                order: copiedQuestion.order,
                options: processedOptions
            };

            await surveyApi.addQuestion(surveyId, requestData);
            message.success('问题复制成功');
            fetchData();
        } catch (error) {
            message.error('复制问题失败');
            console.error(error);
        }
    };

    // 删除问题
    const handleDeleteQuestion = async (questionId) => {
        try {
            await surveyApi.deleteQuestion(surveyId, questionId);
            message.success('问题删除成功');
            fetchData();
        } catch (error) {
            message.error('删除问题失败');
            console.error(error);
        }
    };

    // 保存问题（新增或更新）
    const handleSaveQuestion = async (questionData) => {
        try {
            if (!surveyId) {
                message.error('问卷ID不能为空');
                return;
            }

            // 准备选项数据
            let processedOptions = [];

            // 根据问题类型处理选项
            if (questionData.type === QuestionType.SingleChoice ||
                questionData.type === QuestionType.MultipleChoice) {
                // 单选题和多选题需要处理选项
                processedOptions = (questionData.options || []).map(option => ({
                    content: option.content,
                    value: option.value || option.content,
                    order: option.order,
                    isDefault: option.isDefault || false,
                    isMatrixRow: option.isMatrixRow || false,
                    isMatrixColumn: option.isMatrixColumn || false
                }));
            } else if (questionData.type === QuestionType.Rating) {
                // 对于评分题，创建两个选项：一个表示最小值，一个表示最大值
                const ratingMin = questionData.ratingMin || 1; // 使用表单的ratingMin值或默认值1
                const ratingMax = questionData.ratingMax || 5; // 使用表单的ratingMax值或默认值5

                // 创建选项数组
                processedOptions = [
                    {
                        content: `${ratingMin}分`,
                        value: `${ratingMin}`,
                        order: 0,
                        isDefault: false,
                        isMatrixRow: false,
                        isMatrixColumn: false
                    },
                    {
                        content: `${ratingMax}分`,
                        value: `${ratingMax}`,
                        order: 1,
                        isDefault: false,
                        isMatrixRow: false,
                        isMatrixColumn: false
                    }
                ];

                console.log('Rating question processed options:', processedOptions);
            }

            const requestData = {
                title: questionData.title,
                description: questionData.description,
                type: questionData.type,
                isRequired: questionData.isRequired,
                order: questionData.order,
                options: processedOptions,
                validationRuleType: questionData.validationRuleType,
                customValidationPattern: questionData.customValidationPattern,
                validationErrorMessage: questionData.validationErrorMessage
            };

            console.log('Saving question data:', requestData);

            if (state.isCreating) {
                await surveyApi.addQuestion(surveyId, requestData);
                message.success('问题添加成功');
            } else {
                if (!state.editingQuestion?.id) {
                    message.error('问题ID不能为空');
                    return;
                }
                await surveyApi.updateQuestion(surveyId, state.editingQuestion.id, requestData);
                message.success('问题更新成功');
            }

            state.drawerVisible = false;
            fetchData();
        } catch (error) {
            console.error('保存问题失败:', error);
            message.error(state.isCreating ? '添加问题失败' : '更新问题失败');
            if (error.response?.data?.message) {
                message.error(error.response.data.message);
            }
        }
    };

    // 打开排序抽屉
    const handleOpenSortDrawer = () => {
        // 初始化表单数据
        const initialValues = {};
        state.questions.forEach(question => {
            initialValues[`order_${question.id}`] = question.order;
        });
        form.setFieldsValue(initialValues);
        state.sortDrawerVisible = true;
    };

    // 保存排序
    const handleSaveSort = async () => {
        try {
            const values = await form.validateFields();
            state.sortLoading = true;

            // 创建更新请求
            const updatePromises = state.questions.map(question => {
                const newOrder = values[`order_${question.id}`];
                const requestData = {
                    title: question.title,
                    description: question.description || '',
                    type: question.type,
                    isRequired: question.isRequired,
                    order: newOrder, // 使用表单中的排序值
                    options: question.options || [],
                    validationRuleType: question.validationRuleType,
                    customValidationPattern: question.customValidationPattern,
                    validationErrorMessage: question.validationErrorMessage
                };

                return surveyApi.updateQuestion(surveyId, question.id, requestData);
            });

            // 并行处理所有更新请求
            await Promise.all(updatePromises);
            message.success('问题顺序更新成功');

            // 关闭抽屉并刷新数据
            state.sortDrawerVisible = false;
            fetchData();
        } catch (error) {
            console.error('更新问题顺序失败:', error);
            message.error('更新问题顺序失败');
        } finally {
            state.sortLoading = false;
        }
    };

    // 表格列定义
    const columns = [
        {
            title: '序号',
            dataIndex: 'order',
            key: 'order',
            width: 60,
            render: (_, __, index) => index + 1,
        },
        {
            title: '问题类型',
            dataIndex: 'type',
            key: 'type',
            width: 120,
            render: (type) => {
                const typeMap = {
                    [QuestionType.SingleChoice]: '单选题',
                    [QuestionType.MultipleChoice]: '多选题',
                    [QuestionType.TextInput]: '填空题',
                    [QuestionType.Rating]: '评分题',
                    [QuestionType.MatrixSingleChoice]: '矩阵单选题',
                    [QuestionType.MatrixMultipleChoice]: '矩阵多选题'
                };
                return typeMap[type] || '未知类型';
            },
        },
        {
            title: '问题标题',
            dataIndex: 'title',
            key: 'title',
            ellipsis: {
                showTitle: false,
            },
            render: (title) => (
                <Tooltip color={'blue'} title={title}>
                    {title}
                </Tooltip>
            ),
        },
        {
            title: '必答',
            dataIndex: 'isRequired',
            key: 'isRequired',
            width: 80,
            render: (isRequired) => isRequired ? '是' : '否',
        },
        {
            title: '选项数',
            key: 'optionCount',
            width: 100,
            render: (_, record) => {
                // 对于评分题，显示评分范围而不是选项数
                if (record.type === QuestionType.Rating) {
                    const options = record.options || [];
                    if (options.length >= 2) {
                        return `${options[0].value}-${options[1].value}分`;
                    }
                    return '1-5分';
                }
                return record.options ? record.options.length : 0;
            },
        },
        {
            title: '操作',
            key: 'action',
            width: 200,
            render: (_, record) => (
                <Space>
                    <Button
                        type="text"
                        icon={<EditOutlined />}
                        onClick={() => handleEditQuestion(record)}
                    />
                    <Button
                        type="text"
                        icon={<CopyOutlined />}
                        onClick={() => handleCopyQuestion(record)}
                    />
                    <Popconfirm
                        title="确定要删除这个问题吗?"
                        onConfirm={() => handleDeleteQuestion(record.id)}
                        okText="确定"
                        cancelText="取消"
                    >
                        <Button
                            type="text"
                            danger
                            icon={<DeleteOutlined />}
                        />
                    </Popconfirm>
                </Space>
            ),
        },
    ];

    return (
        <div>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
                <Title level={4}>
                    {state.survey ? `问题管理: ${state.survey.title}` : '问题管理'}
                </Title>
                <Space>
                    <Button
                        icon={<OrderedListOutlined />}
                        onClick={handleOpenSortDrawer}
                    >
                        调整顺序
                    </Button>
                    <Button
                        type="primary"
                        icon={<PlusOutlined />}
                        onClick={handleAddQuestion}
                    >
                        添加问题
                    </Button>
                </Space>
            </div>

            <ProTable
                columns={columns}
                dataSource={state.questions}
                rowKey="id"
                loading={state.loading}
                pagination={false}
                search={false}
                options={{
                    reload: fetchData,
                    density: true,
                    setting: true,
                }}
                tableAlertRender={false}
            />

            {/* 问题编辑抽屉 */}
            <Drawer
                title={state.isCreating ? "添加问题" : "编辑问题"}
                placement="right"
                width={600}
                onClose={() => state.drawerVisible = false}
                open={state.drawerVisible}
                destroyOnClose
            >
                {state.drawerVisible && (
                    <QuestionEdit
                        question={state.editingQuestion}
                        onSave={handleSaveQuestion}
                        onCancel={() => state.drawerVisible = false}
                    />
                )}
            </Drawer>

            {/* 问题排序抽屉 */}
            <Drawer
                title="调整问题顺序"
                placement="right"
                width={400}
                onClose={() => state.sortDrawerVisible = false}
                open={state.sortDrawerVisible}
                destroyOnClose
                extra={
                    <Space>
                        <Button onClick={() => state.sortDrawerVisible = false}>取消</Button>
                        <Button type="primary" loading={state.sortLoading} onClick={handleSaveSort}>
                            保存
                        </Button>
                    </Space>
                }
            >
                <Form
                    form={form}
                    layout="vertical"
                >
                    <div style={{ marginBottom: 16 }}>
                        请为每个问题设置一个顺序号，值越小排列越靠前。
                    </div>
                    {state.questions.map((question, index) => (
                        <Card
                            key={question.id}
                            size="small"
                            title={`${index + 1}. ${question.title}`}
                            style={{ marginBottom: 12 }}
                        >
                            <Form.Item
                                name={`order_${question.id}`}
                                label="顺序"
                                rules={[{ required: true, message: '请输入顺序' }]}
                            >
                                <InputNumber min={0} style={{ width: '100%' }} />
                            </Form.Item>
                        </Card>
                    ))}
                </Form>
            </Drawer>
        </div>
    );
};

export default QuestionList;
